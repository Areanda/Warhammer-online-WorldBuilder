using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WorldBuilder
{
    public class MythicLoginConfigWriter
    {
        public class Server
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public int Port { get; set; }
            public void Serialize(XmlWriter writer, string serverType)
            {
                writer.WriteStartElement(serverType);
                writer.WriteAttributeString("serverName", Name);
                writer.WriteElementString("Address", Address.ToString());
                writer.WriteElementString("Port", Port.ToString());
                writer.WriteEndElement();//ServerType
            }
        }

        public class Region
        {
            public string Name { get; set; }
            public Server PingServer { get; set; }
            public List<Server> LoginServers { get; set; }
            public void Serialize(XmlWriter writer)
            {
                writer.WriteStartElement("Region");
                writer.WriteAttributeString("regionName", Name);

                PingServer.Serialize(writer, "PingServer");

                writer.WriteStartElement("LoginServerList");

                foreach (var server in LoginServers)
                    server.Serialize(writer, "LoginServer");

                writer.WriteEndElement();//LoginServerList

                writer.WriteEndElement();//Region
            }
        }
        public int ProductId { get; set; }
        public int MessageTimeoutSecs { get; set; }
        public List<Region> Regions { get; set; }

        public byte[] Serialize()
        {
            byte[] data = null;
            using (var stream = new MemoryStream())
            {
                using (XmlTextWriter writer = new XmlTextWriter(new StreamWriter(stream)))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartDocument(true);

                    writer.WriteStartElement("RootElementOfAnyName");
                    writer.WriteStartElement("MythLoginServiceConfig");

                    writer.WriteStartElement("Settings");
                    writer.WriteElementString("ProductId", ProductId.ToString());
                    writer.WriteElementString("MessageTimeoutSecs", MessageTimeoutSecs.ToString());
                    writer.WriteEndElement();//Settings

                    writer.WriteStartElement("RegionList");
                    foreach (var region in Regions)
                    {
                        region.Serialize(writer);
                    }
                    writer.WriteEndElement();//RegionList

                    writer.WriteEndElement();//MythLoginServiceConfig
                    writer.WriteEndElement();//RootElementOfAnyName
                    writer.WriteEndDocument();

                    writer.Flush();
                    writer.WriteString("\r\n");
                }
                data = stream.ToArray();
            }
            return data;
        }

    }
}
