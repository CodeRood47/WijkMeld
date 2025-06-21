using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WijkMeld.App.Model
{
    public class IncidentPhoto
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
   
        public Stream PhotoStream { get; set; }

     
        public ImageSource ImageSource => ImageSource.FromFile(FilePath);

  
        public async Task<Stream> OpenStreamForReadAsync()
        {
         
            return await Task.Run(() => File.OpenRead(FilePath));
        }
    }
}
