using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace GitViz.Logic.Mvvm.Utils
{
    public interface IWpfSystem
    {
        void ShowMessage(String message, Boolean isError);

        Boolean AskYesNoQuestion(String messageToAsk);

        String UserChooseFolder(String startingFolder);
    }

    public class NullWpfSystem : IWpfSystem
    {

        public void ShowMessage(string message, bool isError)
        {
        }


        public bool AskYesNoQuestion(string messageToAsk)
        {
            return true;
        }


        public String UserChooseFolder(String startingFolder)
        {
            return "";
        }
    }

    public class WpfSystem : IWpfSystem
    {

        public void ShowMessage(string message, bool isError)
        {
            MessageBox.Show(message, isError ? "Error" : "Messge", MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
        }


        public Boolean AskYesNoQuestion(string messageToAsk)
        {
            return MessageBox.Show(messageToAsk, "Domanda", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }


        public String UserChooseFolder(String startingFolder)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = String.IsNullOrWhiteSpace(startingFolder)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : startingFolder
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "";
        }
    }
}
