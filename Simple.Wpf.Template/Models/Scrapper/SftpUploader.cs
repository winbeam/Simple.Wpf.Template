using System.IO;
using Renci.SshNet;
using System;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Simple.Wpf.Template;

public class SftpUploader
{
    private string _nasServerAddress = "winbm.synology.me";
    private string _nasUsername = "hometax"; 
    private string _nasPassword = "&7bang99conrntEmd";
    private int _port = 5522;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    SftpClient _client;
    public SftpUploader()
    {
        _logger.Log(NLog.LogLevel.Info, "SftpUploader Started");
    }
    public void Connect()
    {
        var connectionInfo = new ConnectionInfo(_nasServerAddress, 5522, _nasUsername,
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(_nasUsername, _nasPassword)
                });

        _client = new SftpClient(connectionInfo);
        _client.Connect();
    }

    static List<string> FilesInFolder(string folderPath)
    {
        string[] files = Array.Empty<string>();
        try
        {
            files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }

        return files.ToList();
    }


    // ex:UploadFileToNas(@"C:\Users\winbe\pw.txt", "/home/xx");
    public bool Upload(AuthInfo authInfo, string folderPath, string remoteFolderPath)
    {
        try
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            var files = FilesInFolder(folderPath);
            if (files.Count == 0)
                throw new Exception($"no files found in {folderPath} folder");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var connectionInfo = new ConnectionInfo(_nasServerAddress, _port, _nasUsername,
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(_nasUsername, _nasPassword)
                });

            using (var sftpClient = new SftpClient(connectionInfo))
            {
                sftpClient.Connect();

                foreach(var file in files)
                {
                    var name = Path.GetFileName(file);
                    var remoteFileFullPath = $"{remoteFolderPath}/{date}_{authInfo.Name}_{authInfo.Birth}_{authInfo.Phone}_{name}";
                    using (var fileStream = new FileStream(file, FileMode.Open))
                    {
                        sftpClient.UploadFile(fileStream, remoteFileFullPath);
                    }
                }

                sftpClient.Disconnect();
            }

            stopwatch.Stop();
            _logger.Info($"Upload 완료 ({string.Join(",", files)}) (elapsed: {stopwatch.Elapsed})");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
        return false;
    }
}
