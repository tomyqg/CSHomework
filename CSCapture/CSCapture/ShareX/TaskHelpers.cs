﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2017 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using ShareX.HelpersLib;




using ShareX.Properties;
using ShareX.ScreenCaptureLib;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace ShareX
{
    public static class TaskHelpers
    {
        public static void ExecuteJob(HotkeyType job, CLICommand command = null)
        {
            ExecuteJob(TaskSettings.GetDefaultTaskSettings(), job, command);
        }

        public static void ExecuteJob(TaskSettings taskSettings)
        {
            ExecuteJob(taskSettings, taskSettings.Job);
        }

        public static void ExecuteJob(TaskSettings taskSettings, HotkeyType job, CLICommand command = null)
        {
            if (job == HotkeyType.None) return;

            DebugHelper.WriteLine("Executing: " + job.GetLocalizedDescription());

            TaskSettings safeTaskSettings = TaskSettings.GetSafeTaskSettings(taskSettings);

            switch (job)
            {

                // Screen capture
                case HotkeyType.PrintScreen:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.Fullscreen, safeTaskSettings, false);
                    break;
                case HotkeyType.ActiveWindow:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.ActiveWindow, safeTaskSettings, false);
                    break;
                case HotkeyType.ActiveMonitor:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.ActiveMonitor, safeTaskSettings, false);
                    break;
                case HotkeyType.RectangleRegion:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.Region, safeTaskSettings, false);
                    break;
                case HotkeyType.RectangleLight:
                    CaptureTaskHelpers.CaptureRectangleLight(safeTaskSettings, false);
                    break;
                case HotkeyType.RectangleTransparent:
                    CaptureTaskHelpers.CaptureRectangleTransparent(safeTaskSettings, false);
                    break;
                case HotkeyType.CustomRegion:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.CustomRegion, safeTaskSettings, false);
                    break;
                case HotkeyType.LastRegion:
                    CaptureTaskHelpers.CaptureScreenshot(CaptureType.LastRegion, safeTaskSettings, false);
                    break;
                case HotkeyType.ScrollingCapture:
                    OpenScrollingCapture(safeTaskSettings, true);
                    break;
                case HotkeyType.CaptureWebpage:
                    OpenWebpageCapture(safeTaskSettings);
                    break;
                
                case HotkeyType.AutoCapture:
                    OpenAutoCapture();
                    break;
                case HotkeyType.StartAutoCapture:
                    StartAutoCapture();
                    break;
                // Screen record
                case HotkeyType.ScreenRecorder:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderActiveWindow:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderCustomRegion:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorder:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIF:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFActiveWindow:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFCustomRegion:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorderGIF:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
                    break;
                case HotkeyType.AbortScreenRecording:
                    AbortScreenRecording();
                    break;
                // Tools
                case HotkeyType.ColorPicker:
                    OpenColorPicker();
                    break;
                case HotkeyType.ScreenColorPicker:
                    OpenScreenColorPicker(safeTaskSettings);
                    break;
                case HotkeyType.ImageEditor:
                    if (command != null && !string.IsNullOrEmpty(command.Parameter) && File.Exists(command.Parameter))
                    {
                        AnnotateImage(command.Parameter, safeTaskSettings);
                    }
                    else
                    {
                        AnnotateImage(safeTaskSettings);
                    }
                    break;
               
                    break;
                case HotkeyType.HashCheck:
                    OpenHashCheck();
                    break;
                case HotkeyType.DNSChanger:
                    OpenDNSChanger();
                    break;
             
                case HotkeyType.IndexFolder:
                  
                    break;
                case HotkeyType.ImageCombiner:
                    OpenImageCombiner(safeTaskSettings);
                    break;
                case HotkeyType.VideoThumbnailer:
                    OpenVideoThumbnailer(safeTaskSettings);
                    break;
                
                // Other
                case HotkeyType.DisableHotkeys:
                    ToggleHotkeys();
                    break;
               
                case HotkeyType.ToggleActionsToolbar:
                    ToggleActionsToolbar();
                    break;
                
            }
        }

        public static ImageData PrepareImage(Image img, TaskSettings taskSettings)
        {
            ImageData imageData = new ImageData();
            imageData.ImageStream = SaveImageAsStream(img, taskSettings.ImageSettings.ImageFormat, taskSettings);
            imageData.ImageFormat = taskSettings.ImageSettings.ImageFormat;

            if (taskSettings.ImageSettings.ImageAutoUseJPEG && taskSettings.ImageSettings.ImageFormat != EImageFormat.JPEG &&
                imageData.ImageStream.Length > taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000)
            {
                imageData.ImageStream.Dispose();
                imageData.ImageStream = SaveImageAsStream(img, EImageFormat.JPEG, taskSettings);
                imageData.ImageFormat = EImageFormat.JPEG;
            }

            return imageData;
        }

        public static string CreateThumbnail(Image img, string folder, string filename, TaskSettings taskSettings)
        {
            if ((taskSettings.ImageSettings.ThumbnailWidth > 0 || taskSettings.ImageSettings.ThumbnailHeight > 0) && (!taskSettings.ImageSettings.ThumbnailCheckSize ||
                (img.Width > taskSettings.ImageSettings.ThumbnailWidth && img.Height > taskSettings.ImageSettings.ThumbnailHeight)))
            {
                string thumbnailFileName = Path.GetFileNameWithoutExtension(filename) + taskSettings.ImageSettings.ThumbnailName + ".jpg";
                string thumbnailFilePath = CheckFilePath(folder, thumbnailFileName, taskSettings);

                if (!string.IsNullOrEmpty(thumbnailFilePath))
                {
                    Image thumbImage = null;

                    try
                    {
                        thumbImage = (Image)img.Clone();
                        thumbImage = new Resize(taskSettings.ImageSettings.ThumbnailWidth, taskSettings.ImageSettings.ThumbnailHeight).Apply(thumbImage);
                        thumbImage = ImageHelpers.FillBackground(thumbImage, Color.White);
                        thumbImage.SaveJPG(thumbnailFilePath, 90);
                        return thumbnailFilePath;
                    }
                    finally
                    {
                        if (thumbImage != null)
                        {
                            thumbImage.Dispose();
                        }
                    }
                }
            }

            return null;
        }

        public static MemoryStream SaveImageAsStream(Image img, EImageFormat imageFormat, TaskSettings taskSettings)
        {
            return SaveImageAsStream(img, imageFormat, taskSettings.ImageSettings.ImageJPEGQuality, taskSettings.ImageSettings.ImageGIFQuality);
        }

        public static MemoryStream SaveImageAsStream(Image img, EImageFormat imageFormat, int jpegQuality = 90, GIFQuality gifQuality = GIFQuality.Default)
        {
            MemoryStream stream = new MemoryStream();

            switch (imageFormat)
            {
                case EImageFormat.PNG:
                    img.Save(stream, ImageFormat.Png);
                    break;
                case EImageFormat.JPEG:
                    try
                    {
                        img = (Image)img.Clone();
                        img = ImageHelpers.FillBackground(img, Color.White);
                        img.SaveJPG(stream, jpegQuality);
                    }
                    finally
                    {
                        if (img != null) img.Dispose();
                    }
                    break;
                case EImageFormat.GIF:
                    img.SaveGIF(stream, gifQuality);
                    break;
                case EImageFormat.BMP:
                    img.Save(stream, ImageFormat.Bmp);
                    break;
                case EImageFormat.TIFF:
                    img.Save(stream, ImageFormat.Tiff);
                    break;
            }

            return stream;
        }

        public static void SaveImageAsFile(Image img, TaskSettings taskSettings)
        {
            using (ImageData imageData = PrepareImage(img, taskSettings))
            {
                string fileName = GetFilename(taskSettings, imageData.ImageFormat.GetDescription(), img);
                string filePath = CheckFilePath(taskSettings.CaptureFolder, fileName, taskSettings);

                if (!string.IsNullOrEmpty(filePath))
                {
                    imageData.Write(filePath);
                    DebugHelper.WriteLine("Image saved to file: " + filePath);
                }
            }
        }

        public static string GetFilename(TaskSettings taskSettings, string extension = null, Image image = null)
        {
            string filename;

            NameParser nameParser = new NameParser(NameParserType.FileName)
            {
                AutoIncrementNumber = Program.Settings.NameParserAutoIncrementNumber,
                MaxNameLength = taskSettings.AdvancedSettings.NamePatternMaxLength,
                MaxTitleLength = taskSettings.AdvancedSettings.NamePatternMaxTitleLength,
                CustomTimeZone = taskSettings.UploadSettings.UseCustomTimeZone ? taskSettings.UploadSettings.CustomTimeZone : null
            };

            if (image != null)
            {
                nameParser.ImageWidth = image.Width;
                nameParser.ImageHeight = image.Height;

                ImageTag imageTag = image.Tag as ImageTag;

                if (imageTag != null)
                {
                    nameParser.WindowText = imageTag.WindowTitle;
                    nameParser.ProcessName = imageTag.ProcessName;
                }
            }

            if (!string.IsNullOrEmpty(nameParser.WindowText))
            {
                filename = nameParser.Parse(taskSettings.UploadSettings.NameFormatPatternActiveWindow);
            }
            else
            {
                filename = nameParser.Parse(taskSettings.UploadSettings.NameFormatPattern);
            }

            Program.Settings.NameParserAutoIncrementNumber = nameParser.AutoIncrementNumber;

            if (!string.IsNullOrEmpty(extension))
            {
                filename += "." + extension.TrimStart('.');
            }

            return filename;
        }

        public static bool ShowAfterCaptureForm(TaskSettings taskSettings, out string fileName, Image img = null, string filePath = null)
        {
            fileName = null;

            if (taskSettings.AfterCaptureJob.HasFlag(AfterCaptureTasks.ShowAfterCaptureWindow))
            {
                AfterCaptureForm afterCaptureForm = null;

                try
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        afterCaptureForm = new AfterCaptureForm(filePath, taskSettings);
                    }
                    else
                    {
                        afterCaptureForm = new AfterCaptureForm(img, taskSettings);
                    }

                    if (afterCaptureForm.ShowDialog() == DialogResult.Cancel)
                    {
                        if (img != null)
                        {
                            img.Dispose();
                        }

                        return false;
                    }

                    fileName = afterCaptureForm.FileName;
                }
                finally
                {
                    afterCaptureForm.Dispose();
                }
            }

            return true;
        }

      

       
        public static void AddDefaultExternalPrograms(TaskSettings taskSettings)
        {
            if (taskSettings.ExternalPrograms == null)
            {
                taskSettings.ExternalPrograms = new List<ExternalProgram>();
            }

            AddExternalProgramFromRegistry(taskSettings, "Paint", "mspaint.exe");
            AddExternalProgramFromRegistry(taskSettings, "Paint.NET", "PaintDotNet.exe");
            AddExternalProgramFromRegistry(taskSettings, "Adobe Photoshop", "Photoshop.exe");
            AddExternalProgramFromRegistry(taskSettings, "IrfanView", "i_view32.exe");
            AddExternalProgramFromRegistry(taskSettings, "XnView", "xnview.exe");
            AddExternalProgramFromFile(taskSettings, "OptiPNG", "optipng.exe");
        }

        private static void AddExternalProgramFromFile(TaskSettings taskSettings, string name, string filename, string args = "")
        {
            if (!taskSettings.ExternalPrograms.Exists(x => x.Name == name))
            {
                if (File.Exists(filename))
                {
                    DebugHelper.WriteLine("Found program: " + filename);

                    taskSettings.ExternalPrograms.Add(new ExternalProgram(name, filename, args));
                }
            }
        }

        private static void AddExternalProgramFromRegistry(TaskSettings taskSettings, string name, string filename)
        {
            if (!taskSettings.ExternalPrograms.Exists(x => x.Name == name))
            {
                ExternalProgram externalProgram = RegistryHelpers.FindProgram(name, filename);

                if (externalProgram != null)
                {
                    taskSettings.ExternalPrograms.Add(externalProgram);
                }
            }
        }

        public static Icon GetProgressIcon(int percentage)
        {
            using (Bitmap bmp = new Bitmap(16, 16))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);

                int width = (int)(16 * (percentage / 100f));

                if (width > 0)
                {
                    using (Brush brush = new LinearGradientBrush(new Rectangle(0, 0, width, 16), Color.DarkBlue, Color.DodgerBlue, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, 0, 0, width, 16);
                    }
                }

                using (Font font = new Font("Arial", 11, GraphicsUnit.Pixel))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(percentage.ToString(), font, Brushes.White, 8, 8, sf);
                }

                g.DrawRectangleProper(Pens.WhiteSmoke, 0, 0, 16, 16);

                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        public static string CheckFilePath(string folder, string filename, TaskSettings taskSettings)
        {
            string filepath = Path.Combine(folder, filename);

            if (File.Exists(filepath))
            {
                switch (taskSettings.ImageSettings.FileExistAction)
                {
                    case FileExistAction.Ask:
                        using (FileExistForm form = new FileExistForm(filepath))
                        {
                            form.ShowDialog();
                            filepath = form.Filepath;
                        }
                        break;
                    case FileExistAction.UniqueName:
                        filepath = Helpers.GetUniqueFilePath(filepath);
                        break;
                    case FileExistAction.Cancel:
                        filepath = "";
                        break;
                }
            }

            return filepath;
        }

        public static void OpenDropWindow(TaskSettings taskSettings = null)
        {
            DropForm.GetInstance(Program.Settings.DropSize, Program.Settings.DropOffset, Program.Settings.DropAlignment, Program.Settings.DropOpacity,
                Program.Settings.DropHoverOpacity, taskSettings).ForceActivate();
        }

        public static void StartScreenRecording(ScreenRecordOutput outputType, ScreenRecordStartMethod startMethod, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ScreenRecordManager.StartStopRecording(outputType, startMethod, taskSettings);
        }

        public static void AbortScreenRecording()
        {
            ScreenRecordManager.AbortRecording();
        }

        public static void OpenScrollingCapture(TaskSettings taskSettings = null, bool forceSelection = false)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ScrollingCaptureForm scrollingCaptureForm = new ScrollingCaptureForm(taskSettings.CaptureSettingsReference.ScrollingCaptureOptions,
                taskSettings.CaptureSettings.SurfaceOptions, forceSelection);
            scrollingCaptureForm.ImageProcessRequested += img => UploadManager.RunImageTask(img, taskSettings);
            scrollingCaptureForm.Show();
        }

        public static void OpenAutoCapture()
        {
            AutoCaptureForm.Instance.ForceActivate();
        }

    
      

        

       

      

      
       

   
        public static void OpenHashCheck()
        {
            new HashCheckForm().Show();
        }

        public static void OpenDirectoryIndexer(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            IndexerSettings indexerSettings = taskSettings.ToolsSettingsReference.IndexerSettings;
            indexerSettings.BinaryUnits = Program.Settings.BinaryUnits;
            DirectoryIndexerForm form = new DirectoryIndexerForm(indexerSettings);
            form.UploadRequested += source =>
            {
                WorkerTask task = WorkerTask.CreateTextUploaderTask(source, taskSettings);
                task.Info.FileName = Path.ChangeExtension(task.Info.FileName, indexerSettings.Output.ToString().ToLowerInvariant());
                TaskManager.Start(task);
            };
            form.Show();
        }

        public static void OpenImageCombiner(TaskSettings taskSettings = null, IEnumerable<string> imageFiles = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ImageCombinerForm imageCombinerForm = new ImageCombinerForm(taskSettings.ToolsSettingsReference.ImageCombinerOptions, imageFiles);
            imageCombinerForm.ProcessRequested += img => UploadManager.RunImageTask(img, taskSettings);
            imageCombinerForm.Show();
        }

        public static void OpenVideoThumbnailer(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!CheckFFmpeg(taskSettings))
            {
                return;
            }

             
            
        }

        public static void AnnotateImage(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (Clipboard.ContainsImage() && MessageBox.Show(Resources.TaskHelpers_OpenImageEditor_Your_clipboard_contains_image,
                Resources.TaskHelpers_OpenImageEditor_Image_editor___How_to_load_image_, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (Image img = ClipboardHelpers.GetImage())
                {
                    if (img != null)
                    {
                        if (taskSettings.AdvancedSettings.UseShareXForAnnotation)
                        {
                            AnnotateImageUsingShareX(img, null, taskSettings);
                        }
                        else
                        {
                            AnnotateImageUsingGreenshot(img, null);
                        }

                        return;
                    }
                }
            }

            string filePath = ImageHelpers.OpenImageFileDialog();

            if (!string.IsNullOrEmpty(filePath))
            {
                AnnotateImage(filePath, taskSettings);
            }
        }

        public static void AnnotateImage(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                if (taskSettings.AdvancedSettings.UseShareXForAnnotation)
                {
                    AnnotateImageUsingShareX(null, filePath, taskSettings);
                }
                else
                {
                    AnnotateImageUsingGreenshot(null, filePath);
                }
            }
            else
            {
                MessageBox.Show("File does not exist:" + Environment.NewLine + filePath, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void AnnotateImageUsingShareX(Image img, string filePath, TaskSettings taskSettings = null)
        {
            if (img == null && File.Exists(filePath))
            {
                img = ImageHelpers.LoadImage(filePath);
            }

            if (img != null)
            {
                using (img)
                {
                    RegionCaptureTasks.AnnotateImage(img, filePath, taskSettings.CaptureSettingsReference.SurfaceOptions,
                        x => UploadManager.RunImageTask(x, taskSettings),
                        (x, newFilePath) => ImageHelpers.SaveImage(x, newFilePath),
                        (x, newFilePath) => ImageHelpers.SaveImageFileDialog(x, newFilePath),
                        x => ClipboardHelpers.CopyImage(x),
                        x => UploadManager.UploadImage(x),
                        x => PrintImage(x));
                }
            }
        }

      
      

     
       
       

       

      

      

      

       

       

   
       
        public static EDataType FindDataType(string filePath, TaskSettings taskSettings)
        {
            if (Helpers.CheckExtension(filePath, taskSettings.AdvancedSettings.ImageExtensions))
            {
                return EDataType.Image;
            }

            if (Helpers.CheckExtension(filePath, taskSettings.AdvancedSettings.TextExtensions))
            {
                return EDataType.Text;
            }

            return EDataType.File;
        }

        public static bool ToggleHotkeys()
        {
            bool hotkeysDisabled = !Program.Settings.DisableHotkeys;

            Program.Settings.DisableHotkeys = hotkeysDisabled;
            Program.HotkeyManager.ToggleHotkeys(hotkeysDisabled);
            Program.MainForm.UpdateToggleHotkeyButton();

            if (Program.MainForm.niTray.Visible)
            {
                Program.MainForm.niTray.Tag = null;
                string balloonTipText = hotkeysDisabled ? Resources.TaskHelpers_ToggleHotkeys_Hotkeys_disabled_ : Resources.TaskHelpers_ToggleHotkeys_Hotkeys_enabled_;
                Program.MainForm.niTray.ShowBalloonTip(3000, "ShareX", balloonTipText, ToolTipIcon.Info);
            }

            return hotkeysDisabled;
        }

        public static bool CheckFFmpeg(TaskSettings taskSettings)
        {
            string ffmpegPath = taskSettings.CaptureSettings.FFmpegOptions.FFmpegPath;

            if (string.IsNullOrEmpty(ffmpegPath))
            {
                ffmpegPath = Program.DefaultFFmpegFilePath;
            }

            if (!File.Exists(ffmpegPath))
            {
                if (MessageBox.Show(string.Format(Resources.ScreenRecordForm_StartRecording_does_not_exist, ffmpegPath),
                    "ShareX - " + Resources.ScreenRecordForm_StartRecording_Missing + " ffmpeg.exe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (FFmpegDownloader.DownloadFFmpeg(false, DownloaderForm_InstallRequested) == DialogResult.OK)
                    {
                        Program.DefaultTaskSettings.CaptureSettings.FFmpegOptions.CLIPath = taskSettings.TaskSettingsReference.CaptureSettings.FFmpegOptions.CLIPath =
                            taskSettings.CaptureSettings.FFmpegOptions.CLIPath = Program.DefaultFFmpegFilePath;

#if STEAM
                        Program.DefaultTaskSettings.CaptureSettings.FFmpegOptions.OverrideCLIPath = taskSettings.TaskSettingsReference.CaptureSettings.FFmpegOptions.OverrideCLIPath =
                          taskSettings.CaptureSettings.FFmpegOptions.OverrideCLIPath = true;
#endif
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static void DownloaderForm_InstallRequested(string filePath)
        {
            bool result = FFmpegDownloader.ExtractFFmpeg(filePath, Program.DefaultFFmpegFilePath);

            if (result)
            {
                MessageBox.Show(Resources.ScreenRecordForm_DownloaderForm_InstallRequested_FFmpeg_successfully_downloaded_, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(Resources.ScreenRecordForm_DownloaderForm_InstallRequested_Download_of_FFmpeg_failed_, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void PlayCaptureSound(TaskSettings taskSettings)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!string.IsNullOrEmpty(taskSettings.AdvancedSettings.SpeechCapture))
            {
                TextToSpeechAsync(taskSettings.AdvancedSettings.SpeechCapture);
            }
            else if (taskSettings.AdvancedSettings.UseCustomCaptureSound && !string.IsNullOrEmpty(taskSettings.AdvancedSettings.CustomCaptureSoundPath))
            {
                Helpers.PlaySoundAsync(taskSettings.AdvancedSettings.CustomCaptureSoundPath);
            }
            else
            {
                Helpers.PlaySoundAsync(Resources.CaptureSound);
            }
        }

        public static void PlayTaskCompleteSound(TaskSettings taskSettings)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!string.IsNullOrEmpty(taskSettings.AdvancedSettings.SpeechTaskCompleted))
            {
                TextToSpeechAsync(taskSettings.AdvancedSettings.SpeechTaskCompleted);
            }
            else if (taskSettings.AdvancedSettings.UseCustomTaskCompletedSound && !string.IsNullOrEmpty(taskSettings.AdvancedSettings.CustomTaskCompletedSoundPath))
            {
                Helpers.PlaySoundAsync(taskSettings.AdvancedSettings.CustomTaskCompletedSoundPath);
            }
            else
            {
                Helpers.PlaySoundAsync(Resources.TaskCompletedSound);
            }
        }

        public static void PlayErrorSound(TaskSettings taskSettings)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (taskSettings.AdvancedSettings.UseCustomErrorSound && !string.IsNullOrEmpty(taskSettings.AdvancedSettings.CustomErrorSoundPath))
            {
                Helpers.PlaySoundAsync(taskSettings.AdvancedSettings.CustomErrorSoundPath);
            }
            else
            {
                Helpers.PlaySoundAsync(Resources.ErrorSound);
            }
        }

        public static void TextToSpeechAsync(string text)
        {
            TaskEx.Run(() =>
            {
                using (SpeechSynthesizer speaker = new SpeechSynthesizer())
                {
                    speaker.Speak(text);
                }
            });
        }

      
        public static Image FindMenuIcon<T>(int index)
        {
            T e = Helpers.GetEnumFromIndex<T>(index);

            if (e is AfterCaptureTasks)
            {
                switch ((AfterCaptureTasks)(object)e)
                {
                    case AfterCaptureTasks.ShowQuickTaskMenu:
                        return Resources.ui_menu_blue;
                    case AfterCaptureTasks.ShowAfterCaptureWindow:
                        return Resources.application_text_image;
                    case AfterCaptureTasks.AddImageEffects:
                        return Resources.image_saturation;
                    case AfterCaptureTasks.AnnotateImage:
                        return Resources.image_pencil;
                    case AfterCaptureTasks.CopyImageToClipboard:
                        return Resources.clipboard_paste_image;
                    case AfterCaptureTasks.SendImageToPrinter:
                        return Resources.printer;
                    case AfterCaptureTasks.SaveImageToFile:
                        return Resources.disk;
                    case AfterCaptureTasks.SaveImageToFileWithDialog:
                        return Resources.disk_rename;
                    case AfterCaptureTasks.SaveThumbnailImageToFile:
                        return Resources.disk_small;
                    case AfterCaptureTasks.PerformActions:
                        return Resources.application_terminal;
                    case AfterCaptureTasks.CopyFileToClipboard:
                        return Resources.clipboard_block;
                    case AfterCaptureTasks.CopyFilePathToClipboard:
                        return Resources.clipboard_list;
                    case AfterCaptureTasks.ShowInExplorer:
                        return Resources.folder_stand;
                    case AfterCaptureTasks.DoOCR:
                        return Resources.edit_drop_cap;
                    case AfterCaptureTasks.ShowBeforeUploadWindow:
                        return Resources.application__arrow;
                    case AfterCaptureTasks.UploadImageToHost:
                        return Resources.upload_cloud;
                    case AfterCaptureTasks.DeleteFile:
                        return Resources.bin;
                }
            }
            else if (e is AfterUploadTasks)
            {
                switch ((AfterUploadTasks)(object)e)
                {
                    case AfterUploadTasks.ShowAfterUploadWindow:
                        return Resources.application_browser;
                    case AfterUploadTasks.UseURLShortener:
                        return Resources.edit_scale;
                    case AfterUploadTasks.ShareURL:
                        return Resources.globe_share;
                    case AfterUploadTasks.CopyURLToClipboard:
                        return Resources.clipboard_paste_document_text;
                    case AfterUploadTasks.OpenURL:
                        return Resources.globe__arrow;
                    case AfterUploadTasks.ShowQRCode:
                        return Resources.barcode_2d;
                }
            }

            return null;
        }

        public static Image GetHotkeyTypeIcon(HotkeyType hotkeyType)
        {
            switch (hotkeyType)
            {
                default: throw new Exception("Icon missing for hotkey type.");
                case HotkeyType.None: return null;
                // Upload
                case HotkeyType.FileUpload: return Resources.folder_open_document;
                case HotkeyType.FolderUpload: return Resources.folder;
                case HotkeyType.ClipboardUpload: return Resources.clipboard;
                case HotkeyType.ClipboardUploadWithContentViewer: return Resources.clipboard_task;
                case HotkeyType.UploadURL: return Resources.drive;
                case HotkeyType.DragDropUpload: return Resources.inbox;
                case HotkeyType.StopUploads: return Resources.cross_button;
                // Screen capture
                case HotkeyType.PrintScreen: return Resources.layer_fullscreen;
                case HotkeyType.ActiveWindow: return Resources.application_blue;
                case HotkeyType.ActiveMonitor: return Resources.monitor;
                case HotkeyType.RectangleRegion: return Resources.layer_shape;
                case HotkeyType.RectangleLight: return Resources.Rectangle;
                case HotkeyType.RectangleTransparent: return Resources.layer_transparent;
                case HotkeyType.CustomRegion: return Resources.layer__arrow;
                case HotkeyType.LastRegion: return Resources.layers;
                case HotkeyType.ScrollingCapture: return Resources.ui_scroll_pane_image;
                case HotkeyType.CaptureWebpage: return Resources.document_globe;
                case HotkeyType.TextCapture: return Resources.edit_drop_cap;
                case HotkeyType.AutoCapture: return Resources.clock;
                case HotkeyType.StartAutoCapture: return Resources.clock__arrow;
                // Screen record
                case HotkeyType.ScreenRecorder: return Resources.camcorder_image;
                case HotkeyType.ScreenRecorderActiveWindow: return Resources.camcorder__arrow;
                case HotkeyType.ScreenRecorderCustomRegion: return Resources.camcorder__arrow;
                case HotkeyType.StartScreenRecorder: return Resources.camcorder__arrow;
                case HotkeyType.ScreenRecorderGIF: return Resources.film;
                case HotkeyType.ScreenRecorderGIFActiveWindow: return Resources.film__arrow;
                case HotkeyType.ScreenRecorderGIFCustomRegion: return Resources.film__arrow;
                case HotkeyType.StartScreenRecorderGIF: return Resources.film__arrow;
                case HotkeyType.AbortScreenRecording: return Resources.camcorder__exclamation;
                // Tools
                case HotkeyType.ColorPicker: return Resources.color;
                case HotkeyType.ScreenColorPicker: return Resources.pipette;
                case HotkeyType.ImageEditor: return Resources.image_pencil;
                case HotkeyType.ImageEffects: return Resources.image_saturation;
                case HotkeyType.HashCheck: return Resources.application_task;
                case HotkeyType.DNSChanger: return Resources.network_ip;
                case HotkeyType.QRCode: return Resources.barcode_2d;
                case HotkeyType.Ruler: return Resources.ruler_triangle;
                case HotkeyType.IndexFolder: return Resources.folder_tree;
                case HotkeyType.ImageCombiner: return Resources.document_break;
                case HotkeyType.VideoThumbnailer: return Resources.images_stack;
                case HotkeyType.FTPClient: return Resources.application_network;
                case HotkeyType.TweetMessage: return Resources.Twitter;
                case HotkeyType.MonitorTest: return Resources.monitor;
                // Other
                case HotkeyType.DisableHotkeys: return Resources.keyboard__minus;
                case HotkeyType.OpenMainWindow: return Resources.application_home;
                case HotkeyType.OpenScreenshotsFolder: return Resources.folder_open_image;
                case HotkeyType.OpenHistory: return Resources.application_blog;
                case HotkeyType.OpenImageHistory: return Resources.application_icon_large;
                case HotkeyType.ToggleActionsToolbar: return Resources.ui_toolbar__arrow;
                case HotkeyType.ExitShareX: return Resources.cross;
            }
        }

        public static Screenshot GetScreenshot(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            Screenshot screenshot = new Screenshot()
            {
                CaptureCursor = taskSettings.CaptureSettings.ShowCursor,
                CaptureClientArea = taskSettings.CaptureSettings.CaptureClientArea,
                RemoveOutsideScreenArea = true,
                CaptureShadow = taskSettings.CaptureSettings.CaptureShadow,
                ShadowOffset = taskSettings.CaptureSettings.CaptureShadowOffset,
                AutoHideTaskbar = taskSettings.CaptureSettings.CaptureAutoHideTaskbar
            };

            return screenshot;
        }

        public static void AddCustomUploader(string filePath)
        {
            if (Program.UploadersConfig != null)
            {
                try
                {
                    CustomUploaderItem cui = JsonHelpers.DeserializeFromFilePath<CustomUploaderItem>(filePath);

                    if (cui != null)
                    {
                        bool activate = false;

                        if (cui.DestinationType == CustomUploaderDestinationType.None)
                        {
                            DialogResult result = MessageBox.Show($"Would you like to add \"{cui.Name}\" custom uploader?",
                                "ShareX - Custom uploader confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                            if (result == DialogResult.No)
                            {
                                return;
                            }
                        }
                        else
                        {
                            List<string> destinations = new List<string>();
                            if (cui.DestinationType.Has(CustomUploaderDestinationType.ImageUploader)) destinations.Add("images");
                            if (cui.DestinationType.Has(CustomUploaderDestinationType.TextUploader)) destinations.Add("texts");
                            if (cui.DestinationType.Has(CustomUploaderDestinationType.FileUploader)) destinations.Add("files");
                            if (cui.DestinationType.Has(CustomUploaderDestinationType.URLShortener)) destinations.Add("urls");

                            string destinationsText = string.Join("/", destinations);

                            DialogResult result = MessageBox.Show($"Would you like to set \"{cui.Name}\" as the active custom uploader for {destinationsText}?",
                                "ShareX - Custom uploader confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                            if (result == DialogResult.Yes)
                            {
                                activate = true;
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                return;
                            }
                        }

                        Program.UploadersConfig.CustomUploadersList.Add(cui);

                        if (activate)
                        {
                            int index = Program.UploadersConfig.CustomUploadersList.Count - 1;

                            if (cui.DestinationType.Has(CustomUploaderDestinationType.ImageUploader))
                            {
                                Program.UploadersConfig.CustomImageUploaderSelected = index;
                                Program.DefaultTaskSettings.ImageDestination = ImageDestination.CustomImageUploader;
                            }

                            if (cui.DestinationType.Has(CustomUploaderDestinationType.TextUploader))
                            {
                                Program.UploadersConfig.CustomTextUploaderSelected = index;
                                Program.DefaultTaskSettings.TextDestination = TextDestination.CustomTextUploader;
                            }

                            if (cui.DestinationType.Has(CustomUploaderDestinationType.FileUploader))
                            {
                                Program.UploadersConfig.CustomFileUploaderSelected = index;
                                Program.DefaultTaskSettings.FileDestination = FileDestination.CustomFileUploader;
                            }

                            if (cui.DestinationType.Has(CustomUploaderDestinationType.URLShortener))
                            {
                                Program.UploadersConfig.CustomURLShortenerSelected = index;
                                Program.DefaultTaskSettings.URLShortenerDestination = UrlShortenerType.CustomURLShortener;
                            }

                            Program.MainForm.UpdateCheckStates();
                            Program.MainForm.UpdateUploaderMenuNames();

                            if (UploadersConfigForm.IsInstanceActive)
                            {
                                UploadersConfigForm.UpdateCustomUploaderTab();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        public static void OpenActionsToolbar()
        {
            ActionsToolbarForm.Instance.ForceActivate();
        }

        public static void ToggleActionsToolbar()
        {
            if (ActionsToolbarForm.IsInstanceActive)
            {
                ActionsToolbarForm.Instance.Close();
            }
            else
            {
                ActionsToolbarForm.Instance.ForceActivate();
            }
        }
    }
}