﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace Goldenacre.Core
{
    public class Helper
    {


        public static bool CurrentUserIsAdmin()
        {
            var currentIdentity = WindowsIdentity.GetCurrent();
            return currentIdentity != null && new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartProcessElevated(string[] args = null)
        {
            var newArgs = new List<string>();

            newArgs.AddRange(args);
            newArgs.AddRange(Environment.GetCommandLineArgs());

            var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, string.Join(" ", newArgs))
            {
                Verb = "runas"
            };

            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = info
            };

            process.Start();
            process.WaitForExit();
        }

        public static string AppFolder()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            var directoryInfo = new FileInfo(new Uri(assembly.GetName().CodeBase).LocalPath).Directory;
            if (directoryInfo != null)
            {
                if (directoryInfo.Parent != null)
                {
                    return directoryInfo.Parent.FullName;
                }
            }

            return null;
        }

        public static long Ping(string host, int port = 80, int timeoutInSeconds = 15)
        {
            var start = DateTime.UtcNow;

            try
            {
                using (var tcp = new TcpClient())
                {
                    var result = tcp.BeginConnect(host, port, null, null);

                    using (var wait = result.AsyncWaitHandle)
                    {
                        if (!wait.WaitOne(timeoutInSeconds * 1000, false))
                        {
                            tcp.Close();
                        }

                        tcp.EndConnect(result);
                    }
                }
            }
            catch
            {
                //
            }

            return (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds;
        }

        public static Color GenerateRandomColour()
        {
            return Color.FromArgb(StaticRandom.Next(0, 255),
                StaticRandom.Next(0, 255),
                StaticRandom.Next(0, 255));
        }

        public static string GetHostNameFromIp(string ipAddress)
        {
            if ((ipAddress == null) || (ipAddress.Trim().Length <= 0))
            {
                throw new ArgumentNullException("ipAddress");
            }

            var ipEntry = Dns.GetHostEntry(ipAddress);

            var strHostName = ipEntry.HostName.ToString(CultureInfo.CurrentCulture);


            return strHostName;
        }

        public static string PrettyPrint(string xml)
        {
            var result = "";

            var ms = new MemoryStream();
            var w = new XmlTextWriter(ms, Encoding.Unicode);
            var d = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                d.LoadXml(xml);

                w.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                d.WriteContentTo(w);
                w.Flush();
                ms.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                ms.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                var sr = new StreamReader(ms);

                // Extract the text from the StreamReader.
                var formattedXml = sr.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
            }

            ms.Close();
            w.Close();

            return result;
        }

        public static StringCollection GetDomainList()
        {
            var domainList = new StringCollection();

            try
            {
                // instantiate the active directory entry object
                var en = new DirectoryEntry("WinNT:");

                // check that there are child objects
                // loop through the child objects
                foreach (DirectoryEntry child in en.Children)
                {
                    // make sure the child object is a domain
                    if ((child.SchemaClassName != null) && (child.SchemaClassName.ToUpper() == "DOMAIN"))
                    {
                        // add the domain to the collection
                        domainList.Add(child.Name.ToUpper());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return domainList;
        }
    }
}