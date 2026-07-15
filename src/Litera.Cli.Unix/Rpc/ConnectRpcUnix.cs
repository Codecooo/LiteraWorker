// using System.Net.Sockets;
// using Litera.Cli.Core.Rpc;

// namespace Litera.Cli.Unix.Rpc;

// public class ConnectRpcUnix : IConnectRpc
// {
//     /// <summary>
//     /// Connects to the RPC server on systemd service socket
//     /// </summary>
//     /// <returns></returns>
//     public async Task<Stream> ConnectAsync()
//     {
//         var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
//         await socket.ConnectAsync(new UnixDomainSocketEndPoint("/run/litera-worker.sock"));

//         var stream = new NetworkStream(socket, ownsSocket: true);
//         return stream;
//     }
// }
