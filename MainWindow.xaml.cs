using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;


namespace PasswordManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// second commit
    public class EncryptionService
    {
        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
    public partial class MainWindow : Window
    {
        private const string FilePath = @"D:\22DTHD4\passwords.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string domain = txtDomain.Text;
                string password = txtPassword.Password;
                string masterKey = txtMasterKey.Password;

                if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(masterKey))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                // Mã hóa mật khẩu
                string encryptedPassword = EncryptionService.Encrypt(password, masterKey);

                // Lưu vào file
                File.AppendAllText(FilePath, $"{domain}:{encryptedPassword}\n");

                MessageBox.Show("Password saved successfully!");

                // Xóa nội dung
                txtDomain.Clear();
                txtPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        
        private void ViewPasswords_Click(object sender, RoutedEventArgs e)
        {
            string masterKey = txtMasterKey.Password;

            if (masterKey.Length != 16 && masterKey.Length != 24 && masterKey.Length != 32)
            {
                MessageBox.Show("Master key must be 16, 24, or 32 characters long.");
                return;
            }


            // Đọc mật khẩu từ file và giải mã
            lstPasswords.Items.Clear();
            var lines = File.ReadAllLines(FilePath);

            foreach (var line in lines)
            {
                var data = line.Split(':');
                string domain = data[0];
                string encryptedPassword = data[1];

                try
                {
                    string decryptedPassword = EncryptionService.Decrypt(encryptedPassword, masterKey);
                    lstPasswords.Items.Add($"{domain}: {decryptedPassword}");
                }
                catch
                {
                    MessageBox.Show("Incorrect master key!");
                    return;
                }
            }
        }
        private void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra xem có mục nào được chọn hay không
                if (lstPasswords.SelectedItem != null)
                {
                    // Lấy mục đã chọn (chứa domain và mật khẩu)
                    string selectedItem = lstPasswords.SelectedItem.ToString();

                    // Đọc tất cả các dòng trong file
                    var lines = File.ReadAllLines(FilePath).ToList();

                    // Xóa dòng tương ứng với mục đã chọn trong ListBox
                    if (lines.Contains(selectedItem))
                    {
                        lines.Remove(selectedItem);

                        // Ghi lại danh sách đã cập nhật vào file, ghi đè file cũ
                        File.WriteAllLines(FilePath, lines);

                        // Xóa mục trong ListBox
                        lstPasswords.Items.Remove(lstPasswords.SelectedItem);

                        MessageBox.Show("Password deleted successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Password not found in the file.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a password to delete.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }


        private void lstPasswords_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Kiểm tra xem có mục nào được chọn trong ListBox hay không
            if (lstPasswords.SelectedItem != null)
            {
                // Lấy chuỗi chứa domain và mật khẩu từ mục được chọn
                string selectedItem = lstPasswords.SelectedItem.ToString();

                // Domain và mật khẩu được phân tách bởi dấu ':' nên tách domain ra
                string[] parts = selectedItem.Split(':');
                if (parts.Length > 0)
                {
                    string domain = parts[0];

                    // Kiểm tra và định dạng lại domain nếu cần
                    if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                    {
                        domain = "https://" + domain;
                    }

                    // Mở trình duyệt web với domain
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = domain,
                        UseShellExecute = true
                    });
                }
            }
        }
    }

}
