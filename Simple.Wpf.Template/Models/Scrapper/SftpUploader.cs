using System.IO;
using Renci.SshNet;
using System;

public class SftpUploader
{
    private string _nasServerAddress; // NAS 서버 주소
    private string _nasUsername; // NAS 서버 사용자 이름
    private string _nasPassword; // NAS 서버 비밀번호
    public SftpUploader()
    {
        
    }
    public SftpUploader(string serverAddress, string username, string password)
    {
        _nasServerAddress = serverAddress;
        _nasUsername = username;
        _nasPassword = password;
    }
    public void Upload()
    {
        _nasServerAddress = "winbm.synology.me"; // NAS 서버의 주소
        _nasUsername = "hometax"; // NAS 서버의 사용자 이름
        _nasPassword = "bang99con"; // NAS 서버의 비밀번호

        string localFilePath = @"C:\Users\winbe\pw.txt"; // 업로드할 로컬 파일 경로
        string remoteFilePath = "/volume1/share"; // 업로드할 원격 파일 경로

        UploadFileToNas(localFilePath, remoteFilePath);
    }
    public void UploadFileToNas(string localFilePath, string remoteFilePath)
    {
        try
        {
            var connectionInfo = new ConnectionInfo(_nasServerAddress, 5522, _nasUsername,
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(_nasUsername, _nasPassword)
                });

            using (var sftpClient = new SftpClient(connectionInfo))
            {
                sftpClient.Connect();

                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    sftpClient.UploadFile(fileStream, "/home/aa");
                }

                sftpClient.Disconnect();
            }

            Console.WriteLine("파일 업로드 완료");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"파일 업로드 중 오류 발생: {ex.Message}");
        }
    }
}
