using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FocoComunicacion
{
    class FocoUDP
    {
        Socket sck;
        EndPoint epLocal;
        EndPoint epRem;
        String IpLoc;
        String IpRem;
        int PuertoLoc;
        int PuertoRem;
        byte[] buffer;
        public Queue<String> Recibido { get; set; }

        public static string IpLocal()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
            //throw new Exception("Local IP Address Not Found ");
        }

        public FocoUDP(String ip_local = "", int puerto_local = 8888, String ip_remota = "", int puerto_remoto = 0)
        {
            //Inicializar Socket
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if(ip_local.Length == 0)
            {
                IpLoc = IpLocal();
            }
            else
            {
                IpLoc = ip_local;
            }
            PuertoLoc = puerto_local;
            IpRem = ip_remota;
            PuertoRem = puerto_remoto;
            Recibido = new Queue<String>();
        }
        public void Inicializar(bool Recibir = true)
        {
            //unir los sockets
            epLocal = new IPEndPoint(IPAddress.Parse(IpLoc), PuertoLoc);
            sck.Bind(epLocal);

            if (IpRem.Length == 0)
            {
                //conectar a recepcion general remota
                epRem = new IPEndPoint(IPAddress.Any, PuertoRem);
                //sck.Connect(epRem);
            }
            else
            {
                //conectar a la ip remota
                epRem = new IPEndPoint(IPAddress.Parse(IpRem), PuertoRem);
                sck.Connect(epRem);
            }

            if (Recibir)
            {
                //viendo la ruta especifica
                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRem, new AsyncCallback(MessageCallBack), buffer);
            }
        }

        public void Enviar(string Mensaje)
        {
            //convertir el mensaje string a byte
            ASCIIEncoding aEncoding = new ASCIIEncoding();
            byte[] sendingMessage = new byte[1500];
            sendingMessage = aEncoding.GetBytes(Mensaje);
            //enviar para codificar mensaje
            sck.Send(sendingMessage);
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] receivedData = new byte[1500];
                receivedData = (byte[])aResult.AsyncState;

                //de byte a string
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string receivedMessage = aEncoding.GetString(receivedData);

                //Anadirlo al messa listbox
                Recibido.Enqueue(receivedMessage);

                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRem, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception ex)
            {

            }

        }
        public void Desconectar()
        {
            sck.Disconnect(true);
        }
    }
}
