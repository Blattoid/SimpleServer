using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Configuration;

namespace SimpleServer
{
    class Server
    {
        public static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static string DecideWhichSlash()
        {
            //Extremely simple; checks is IsUnix is true and returns the appropriate slash combination. This is to heed the filepath structures.
            if (IsUnix)
            {
                return "/"; //unix
            }
            else
            {
                return "\\"; //windows
            }
        }
        public static bool bannerExists;
        public static bool useWelcomeBanner;
        public static bool hasenteredcorrectpass;
        public static string welcomeBanner;
        public static string welcomeMessage;
        public static string password;
        public static bool usepassword;
        public static int port;
        // Incoming data from the client.  
        public static void Main(string[] args)
        {
            //Read the config file
            try
            {
                //Emergency rountine if the config file is missing
                useWelcomeBanner = true;
                welcomeMessage = "Welcome to the server!";
                welcomeBanner = @"\welcomeBanner.txt";
                password = "password";
                usepassword = false;
                port = 8081;

                //Check if the config file exists
                if (File.Exists(Assembly.GetEntryAssembly().Location + ".config"))
                {
                    useWelcomeBanner = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("useWelcomeBanner"));
                    welcomeBanner = ConfigurationManager.AppSettings.Get("welcomeBannerFilename");
                    welcomeMessage = ConfigurationManager.AppSettings.Get("welcomeMessage");
                    password = ConfigurationManager.AppSettings.Get("password");
                    usepassword = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("usepassword"));
                    port = Convert.ToInt16(ConfigurationManager.AppSettings.Get("port"));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: CONFIG FILE MISSING");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                //check if welcome banner doesn't exist
                if (!File.Exists(Directory.GetCurrentDirectory() + DecideWhichSlash() + welcomeBanner) && useWelcomeBanner)
                {
                    Console.WriteLine("Warning: Welcome banner file '" + welcomeBanner + "' doesn't exist.");
                    bannerExists = false;
                }
                else
                {
                    bannerExists = true;
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error reading config file: " + e);
                Console.ForegroundColor = ConsoleColor.White;
            }

            for (; ; )
            {
                try
                {
                    //if using password, print this to the console.
                    if (usepassword)
                    {
                        Console.WriteLine("Use password enabled.");
                    }
                    //If banner is enabled, print this to the console.
                    if (useWelcomeBanner&&bannerExists)
                    {
                        Console.WriteLine("Use banner enabled.");
                    }
                    Console.WriteLine("\nStarting server...");
                    TcpListener listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    Console.WriteLine("Started server on port " + Convert.ToString(port));
                    Socket socket = listener.AcceptSocket();
                    //connected

                    IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint; //create way of getting addresses
                    Console.WriteLine("Connection established from " + remoteIpEndPoint.Address);

                    //First contact with the aliens! We need to make a good first impression, so send the banner if specified. 
                    if (bannerExists && useWelcomeBanner)
                    {
                        methods.readFile(welcomeBanner, socket);
                    }
                    //Check if we do or don't need a password to connect and send the appropriate message.
                    if (usepassword) { socket.Send(Encoding.ASCII.GetBytes("\nEnter password: ")); }
                    else { socket.Send(Encoding.ASCII.GetBytes(welcomeBanner + "\nType help for a list of commands.\n>")); }

                    for (; ; )
                    {
                        var data_bytes = methods.ReceiveAll(socket);
                        string data = Encoding.UTF8.GetString(data_bytes, 0, data_bytes.Length);

                        if (methods.IsConnected(socket) == false)
                        {
                            //if the user closes the connection without warning this would normally crash the program. This handles it.
                            Console.WriteLine("Disconnected prematurely.\n");
                            socket.Disconnect(false);
                            socket.Dispose();
                            listener.Stop();
                            break;
                        }
                        if (data.Length != 0)
                        {
                            //remove pesky return at the end
                            data = data.Remove(data.Length - 1, 1);

                            //data is now formatted as a single line string.

                            //password checking
                            if (usepassword == false) { hasenteredcorrectpass = true; }
                            else
                            {
                                if (data == password)
                                {
                                    Console.WriteLine("Correct password entered.");
                                    socket.Send(Encoding.ASCII.GetBytes("Welcome! Type help for a list of commands.\n"));
                                    hasenteredcorrectpass = true;
                                    usepassword = false;
                                    data = "BLANK";
                                }
                                else
                                {
                                    Console.WriteLine("Incorrect password entered.");
                                    socket.Send(Encoding.ASCII.GetBytes("Incorrect password.\n"));
                                    hasenteredcorrectpass = false;
                                    socket.Disconnect(false);
                                    socket.Dispose();
                                    listener.Stop();
                                    break;
                                }
                            }
                            if (hasenteredcorrectpass)
                            {
                                Console.WriteLine("Recieved '" + data + "'");
                                if (data.ToUpper() == "EXIT")
                                {
                                    Console.WriteLine("Client waved goodbye\n");
                                    socket.Send(Encoding.ASCII.GetBytes("Goodbye!\n")); //respond
                                    socket.Disconnect(false);
                                    socket.Dispose();
                                    listener.Stop();
                                    break;
                                }
                                else if (data.ToUpper() == "HELP") { socket.Send(Encoding.ASCII.GetBytes("\nList of commands:\n\tHELP\n\t8BALL\n\tDRIVES\n\tCREDITS\n\tEXIT\n")); }
                                else if (data.ToUpper() == "8BALL")
                                {

                                    socket.Send(Encoding.ASCII.GetBytes("I am the magic 8-Ball! Ask me any question! (Type 'exit' to quit)\n?"));
                                    for (; ; )
                                    {
                                        for (; ; )
                                        {
                                            data_bytes = methods.ReceiveAll(socket);
                                            data = Encoding.UTF8.GetString(data_bytes, 0, data_bytes.Length);

                                            if (data.Length != 0) { break; }
                                        }
                                        data = data.Remove(data.Length - 1, 1);
                                        Console.WriteLine("Question: '" + data + "'");

                                        //check for exit command
                                        if (data.ToUpper() == "EXIT")
                                        {
                                            //exit the loop of question asking
                                            break;
                                        }

                                        //they haven't asked to exit, so let's respond with random answer
                                        if (data != "")
                                        {
                                            string[] ballanswers = { "It is certain", "It is decidedly so", "Without a doubt", "Yes definitely", "You may rely on it", "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes", "Reply hazy try again", "Ask again later", "Better not tell you now", "Cannot predict now", "Concentrate and ask again", "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good", "Very doubtful" };
                                            string ballresponse = ballanswers[new Random().Next(0, ballanswers.Length)]; //pick response to send
                                            Console.WriteLine("Responded with '" + ballresponse + "'.");
                                            socket.Send(Encoding.ASCII.GetBytes(ballresponse + "\n?")); //send response
                                        }
                                        else { socket.Send(Encoding.ASCII.GetBytes("?")); }
                                    }
                                    socket.Send(Encoding.ASCII.GetBytes("Goodbye!\n"));
                                }
                                else if (data.ToUpper() == "CREDITS")
                                {
                                    socket.Send(Encoding.ASCII.GetBytes("Made by a random kid on the internet entirely for fun.\nFor more C# projects visit their GitHub: github.com/floathandthing\n"));
                                }
                                else if (data.ToUpper() == "DRIVES") { methods.ListDrives(socket); }
                                else if (data.ToUpper() == "BLANK") { }
                                else if (data.ToUpper() == "") { } //if they type nothing do nothing.
                                else { socket.Send(Encoding.ASCII.GetBytes("Unknown command '" + data + "'.\n")); }
                                socket.Send(Encoding.ASCII.GetBytes(">"));
                            }
                        }
                    }


                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error recieving: " + e);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.ReadKey();
                }
            }
        }
    }
    public static class methods
    {
        public static void readFile(string filename, Socket socket)
        {
            //filesize is in bytes.
            FileInfo fileinfo = new FileInfo(@filename);
            Int32 filesize = Convert.ToInt32(fileinfo.Length);

            //Write each line of the file to the socket
            foreach (string line in File.ReadAllLines(@filename)) { socket.Send(Encoding.ASCII.GetBytes(line+"\n")); }

            Console.WriteLine("Sent banner.");
        }



        public static void ListDrives(Socket socket)
        {
            //Command which allows access to basic drive information. Because why not.
            try
            {
                DriveInfo[] myDrives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in myDrives)
                {
                    if (drive.IsReady == true)
                    {
                        int kilobyte_size = 1024;
                        int megabyte_size = 1024000;
                        int gigabyte_size = 1024000000;
                        //Int64 is needed due to the numbers being way too big for 32 bit.
                        Int64 terabyte_size = Convert.ToInt64(1024000000000);
                        string size;

                        try
                        {
                            //Calculate readable drive size. Uses kilobytes, megabytes, gigabytes and even terabytes.
                            if (drive.TotalSize > terabyte_size)
                            {
                                //Yikes.
                                float formatted_size = drive.TotalSize / terabyte_size;
                                size = (formatted_size.ToString() + " TB");
                            }
                            else if (drive.TotalSize > gigabyte_size)
                            {
                                //Expectable
                                float formatted_size = drive.TotalSize / gigabyte_size;
                                size = (formatted_size.ToString() + " GB");
                            }
                            else if (drive.TotalSize > megabyte_size)
                            {
                                //Uncommon.
                                float formatted_size = drive.TotalSize / megabyte_size;
                                size = (formatted_size.ToString() + " MB");
                            }
                            else if (drive.TotalSize > kilobyte_size)
                            {
                                //Rare. Must be using a floppy drive or something.
                                float formatted_size = drive.TotalSize / kilobyte_size;
                                size = (formatted_size.ToString() + " KB");
                            }
                            else
                            {
                                //Simply return the size in bytes. We tried.
                                size = (drive.TotalSize + " B");
                            }
                        }
                        catch (DivideByZeroException) { size = "ERR"; }

                        //Calculate free space percentage. Uses sorcery to convert long data type into int.
                        double freespacepercentage = 100 * (double)drive.TotalFreeSpace / drive.TotalSize;
                        //Round to 1 decimal place
                        freespacepercentage = Convert.ToDouble(freespacepercentage.ToString("n1"));
                        Console.WriteLine(drive.Name + " (" + drive.VolumeLabel + ") " + freespacepercentage + "% free of " + size + ".");
                        socket.Send(Encoding.ASCII.GetBytes(drive.Name + " (" + drive.VolumeLabel + ") " + freespacepercentage + "% free of " + size + ".\n"));
                    }

                    else { socket.Send(Encoding.ASCII.GetBytes(drive.Name)); }
                }
            }
            catch (Exception) { throw; }

        }
        public static bool IsConnected(this Socket socket)
        {
            try { return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0); }
            catch (SocketException) { return false; }
        }
        public static byte[] ReceiveAll(this Socket socket)
        {
            var buffer = new List<byte>();

            while (socket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1))
                {
                    buffer.Add(currByte[0]);
                }
            }
            return buffer.ToArray();
        }
    }
}