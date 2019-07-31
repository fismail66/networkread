using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TcpChannelCT
{

    public CancellationTokenSource cts { get; set; }
    public TcpClient Channel { get; set; }
    public int TcpPort { get; set; }
    public string ServerIp { get; set; }
    public bool disconnect = true;

    public async Task ConnectAsyncCH()
    {
        cts = new CancellationTokenSource();
        disconnect = false;
        Channel = new TcpClient();
        await Channel.ConnectAsync(IPAddress.Parse(ServerIp), Convert.ToInt16(TcpPort)).ContinueWith(async task =>
        {
            if (task.IsCanceled)
            {

            }
            if (task.IsFaulted)
            {
                Console.WriteLine(task.Exception.Flatten().InnerException.Message);
                await Reconnect();
            }
            else
            {
                if (task.IsCompleted)
                    await ReceiveAsync();
            }

        });

    }

    public async Task ReceiveAsync()
    {
        using (Channel)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                byte[] buf = new byte[1];
                var stream = Channel.GetStream();
                string sequence = string.Empty;
                StringBuilder sb = new StringBuilder();
                //string end = "\r\n";
                string end = "\r";
                int amountRead;
                CancellationTokenSource readcts = new CancellationTokenSource();

                while (!cts.Token.IsCancellationRequested)
                {
                    amountRead = await stream.ReadAsync(buf, 0, buf.Length, cts.Token);
                    if (amountRead == 0)
                    {
                        Trace("Connection lost from server");
                        break; //end of stream.
                    }
                    sequence = Encoding.ASCII.GetString(buf, 0, 1);
                    sb.Append(sequence);

                    int timeout = 5000;
                    var task = ReadCharByChar(sb, stream, end, readcts.Token);
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                            sequence = sb.ToString();
                            sb.Clear();
                            //Trace( "Seq end character : " + sequence);
                            LogHelper.Log(LogTarget.File, sequence);
                        if (task.IsCanceled)
                        {
                            Trace("Read task canceled");
                        }

                        if (task.IsFaulted)
                        {
                            Trace("Read task faulted");
                        }
                    }
                    else
                    {
                            // timeout logic
                            sequence = sb.ToString();
                            sb.Clear();
                            Trace("Seq after timeout" + sequence);
                    }


                }
            }
            catch (Exception ex)
            {

                Trace(ex.GetBaseException().Message);
                await Reconnect();
            }

        }
    }

    public async Task<Boolean> ReadCharByChar(StringBuilder sb, NetworkStream stream, string end, CancellationToken token)
    {
        int amountRead;
        byte[] buf = new byte[1];
        string bufread;
        string sequence = string.Empty;
        bool result = true;
        do
        {
            amountRead = await stream.ReadAsync(buf, 0, 1, cts.Token);
            if (amountRead == 0)
            {
                Trace("Connection lost from server");
                break; //end of stream.
            }
            bufread = Encoding.ASCII.GetString(buf, 0, 1);
            sb.Append(bufread);
        } while (!sb.ToString().Contains(end) || token.IsCancellationRequested);

        return result;
    }

    public async Task WriteAsync(string message)
    {
        using (Channel)
        {
            var stream = Channel.GetStream();
            var msg = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(msg, 0, msg.Length)
                .ConfigureAwait(false);
        }
    }

    public void Disconnect()
    {
        disconnect = true;
        cts.Cancel();
        using (Channel)
        {
            Channel.Close();
        }

    }

    public void HandleMessage(string msg)
    {
        Console.WriteLine(msg);
    }

    public void Trace(string message)
    {
        Console.Error.WriteLine(message);
    }

    public async Task Reconnect()
    {
        if (!disconnect)
            await Task.Delay(2000).ContinueWith(async t =>
            {
                await ConnectAsyncCH();
            });
    }
}