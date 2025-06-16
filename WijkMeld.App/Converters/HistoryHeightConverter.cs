using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkMeld.App.Converters
{
   
        public class HistoryHeightConverter : IValueConverter
        {
            // De hoogte van één regel item (ongeveer). Pas dit aan indien nodig.
            private const double ItemHeight = 25; // Geschatte hoogte per Label in de CollectionView

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is ICollection<WijkMeld.App.Model.StatusUpdate> history) // Zorg ervoor dat het type overeenkomt
                {
                    // Bereken de totale hoogte op basis van het aantal items
                    // Voeg een kleine extra padding toe indien gewenst
                    return history.Count * ItemHeight + 10; // 10 is voor wat extra marge
                }
                return 0; // Standaardhoogte als er geen collectie is of het type niet klopt
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    
}
