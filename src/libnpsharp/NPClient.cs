﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NPSharp.RPC;
using NPSharp.RPC.Messages;

namespace NPSharp
{
    /// <summary>
    /// Represents a high-level network platform client.
    /// </summary>
    public class NPClient
    {
        private readonly RPCClientStream _rpc;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _procTask;
		private readonly ILog _log;

        /// <summary>
        /// Initializes the NP client with a specified host and port.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to use. Default: 3025.</param>
        public NPClient(string host, ushort port = 3025)
        {
            _rpc = new RPCClientStream(host, port);
			_log = LogManager.GetLogger ("NPClient");
        }

        /// <summary>
        /// The assigned NP user ID. Will be set on successful authentication.
        /// </summary>
        public ulong LoginId { get; private set; }

        /// <summary>
        /// The assigned session token for this client. Will be set on successful authentication.
        /// </summary>
        public string SessionToken { get; private set; }

        // TODO: Handle connection failures via exception
        /// <summary>
        /// Connects the client to the NP server.
        /// </summary>
        /// <returns>True if the connection succeeded, otherwise false.</returns>
        public bool Connect()
        {
            _log.Debug("Connect() start");

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            if (!_rpc.Open())
                return false;

            _procTask = Task.Factory.StartNew(() =>
            {
				_log.Debug("Now receiving RPC messages");
                try
                {
                    while (true)
                    {
                        _rpc.Read();
                    }
                }
                catch (ProtocolViolationException error)
                {
                    _log.ErrorFormat("Protocol violation: {0}. Disconnect imminent.", error.Message);
					Disconnect();
                }

				_log.Debug("Now not receiving RPC messages anymore");
            }, _cancellationToken);

            _log.Debug("Connect() done");
            return true;
        }

        /// <summary>
        /// Disconnects the client from the NP server.
        /// </summary>
        public void Disconnect()
        {
            _log.Debug("Disconnect() start");

            _cancellationTokenSource.Cancel(true); // TODO: Find a cleaner way to cancel _processingTask (focus: _rpc.Read)
            //_procTask.Wait(_cancellationToken);
            _rpc.Close();

            LoginId = 0;

            _log.Debug("Disconnect() done");
        }

        // TODO: Try to use an exception for failed action instead
        /// <summary>
        /// Authenticates this connection via a token. This token has to be requested via an external interface like remauth.php.
        /// </summary>
        /// <param name="token">The token to use for authentication</param>
        /// <returns>True if the login succeeded, otherwise false.</returns>
        public async Task<bool> AuthenticateWithToken(string token)
        {
            var tcs = new TaskCompletionSource<bool>();

            _rpc.AttachCallback(packet =>
            {
                var result = packet as AuthenticateResultMessage;
                if (result == null)
                    return;

                if (result.Result != 0)
                    tcs.SetResult(false);
                LoginId = result.NPID;
                SessionToken = result.SessionToken;
                tcs.SetResult(true);
            });
            _rpc.Send(new AuthenticateWithTokenMessage {Token = token});

            return await tcs.Task;
        }

        // TODO: Try to use an exception for failed action instead
        /// <summary>
        /// Uploads a user file.
        /// </summary>
        /// <param name="filename">The file name to save the contents to on the server</param>
        /// <param name="contents">The raw byte contents</param>
        /// <returns>True if the upload succeeded, otherwise false.</returns>
        public async Task<bool> UploadUserFile(string filename, byte[] contents)
        {
            var tcs = new TaskCompletionSource<bool>();

            _rpc.AttachCallback(packet =>
            {
                var result = (StorageWriteUserFileResultMessage) packet;
                if (result.Result != 0)
                    tcs.SetResult(false);
                tcs.SetResult(true);
            });
            _rpc.Send(new StorageWriteUserFileMessage {FileData = contents, FileName = filename, NPID = LoginId});

            return await tcs.Task;
        }
        
        /// <summary>
        /// Downloads a user file and returns its contents.
        /// </summary>
        /// <param name="filename">The file to download</param>
        /// <returns>File contents as byte array</returns>
        public async Task<byte[]> GetUserFile(string filename)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            _rpc.AttachCallback(packet =>
            {
                var result = (StorageUserFileMessage) packet;
                if (result.Result != 0)
                {
                    tcs.SetException(new NpFileException());
                    return;
                }
                tcs.SetResult(result.FileData);
            });
            _rpc.Send(new StorageGetUserFileMessage {FileName = filename, NPID = LoginId});

            return await tcs.Task;
        }


        /// <summary>
        /// Downloads a user file onto the harddisk.
        /// </summary>
        /// <param name="filename">The file to download</param>
        /// <param name="targetpath">Path where to save the file</param>
        public async void DownloadUserFileTo(string filename, string targetpath)
        {
            var contents = await GetUserFile(filename);

            File.WriteAllBytes(targetpath, contents);
        }


        /// <summary>
        /// Downloads a publisher file and returns its contents.
        /// </summary>
        /// <param name="filename">The file to download</param>
        /// <returns>File contents as byte array</returns>
        public async Task<byte[]> GetPublisherFile(string filename)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            _rpc.AttachCallback(packet =>
            {
                var result = (StoragePublisherFileMessage) packet;
                if (result.Result != 0)
                {
                    tcs.SetException(new NpFileException());
                    return;
                }
                tcs.SetResult(result.FileData);
            });
            _rpc.Send(new StorageGetPublisherFileMessage {FileName = filename});

            return await tcs.Task;
        }

        /// <summary>
        /// Downloads a publisher file onto the harddisk.
        /// </summary>
        /// <param name="filename">The file to download</param>
        /// <param name="targetpath">Path where to save the file</param>
        public async void DownloadPublisherFileTo(string filename, string targetpath)
        {
            var contents = await GetPublisherFile(filename);

            File.WriteAllBytes(targetpath, contents);
        }

        public void SendRandomString(string data)
        {
            _rpc.Send(new StorageSendRandomStringMessage() { RandomString=data });
        }
    }
}