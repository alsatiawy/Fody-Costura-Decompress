using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;

namespace Fody_Costura_Decompress
{
    public partial class MainForm : Form
    {
        List<FileInfo> _sourceFiles;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ProcessFile(FileInfo sourceFile, string destinationFileName, bool compress)
        {
            using (var originalFileStream = File.OpenRead(sourceFile.FullName))
            using (var destinationFileStream = File.Create(destinationFileName))
                if (compress)
                    using (var compressionStream = new DeflateStream(destinationFileStream, CompressionMode.Compress))
                        originalFileStream.CopyTo(compressionStream);
                else
                    using (var decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
                        decompressionStream.CopyTo(destinationFileStream);
        }

        private void InputFileButton_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { Multiselect = true };

            var openResult = openDialog.ShowDialog();

            if (openResult == DialogResult.OK)
            {
                selectedFileTextBox.Text = string.Join(";", openDialog.SafeFileNames);
                _sourceFiles = openDialog.FileNames.Select(s => new FileInfo(s)).ToList();
                decompressButton.Enabled = true;
                compressButton.Enabled = true;
            }
        }

        private void DecompressButton_Click(object sender, EventArgs e)
        {
            bool ret = true;
            string errFile = "";
            foreach (var sourceFileInfo in _sourceFiles)
            {
                var sourceFileName = sourceFileInfo.FullName.ToString();
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName);
                var inflatedFileName = sourceFileName.Remove(sourceFileName.Length - sourceFileInfo.Extension.Length);
                try
                {
                    ProcessFile(sourceFileInfo, inflatedFileName, false);
                    System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(inflatedFileName);
                    if (!string.IsNullOrWhiteSpace(info.InternalName))
                        File.Move(inflatedFileName, Path.Combine(sourceFileInfo.DirectoryName, info.InternalName));
                }
                catch (Exception err)
                {
                    ret = false;
                    errFile += sourceFileInfo.Name + ": " + err.Message;
                }
            }
            if (ret)
                MessageBox.Show("Successfully inflated.");
            else
                MessageBox.Show("Error when inflating file: " + System.Environment.NewLine + errFile);
        }

        private void compressButton_Click(object sender, EventArgs e)
        {
            bool ret = true;
            string errFile = "";
            foreach (var sourceFileInfo in _sourceFiles)
            {
                var sourceFileName = sourceFileInfo.FullName.ToString();
                var deflatedFileName = sourceFileName + ".compressed";
                try
                {
                    ProcessFile(sourceFileInfo, deflatedFileName, true);
                }
                catch (Exception err)
                {
                    ret = false;
                    errFile += sourceFileInfo.Name + ": " + err.Message;
                }
            }
            if (ret)
                MessageBox.Show("Successfully deflated.");
            else
                MessageBox.Show("Error when deflated file: " + System.Environment.NewLine + errFile);
        }
    }
}
