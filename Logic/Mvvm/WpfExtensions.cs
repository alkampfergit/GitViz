using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GitViz.Logic.Mvvm
{
    public static class WpfExtensions
    {
        /// <summary>
        /// AMolto spesso io ho una lista di oggetti, ma per i filtri debbo aggiungere il 
        /// valore iniziale, quello cioè dell'elemento non selezionato altrmenti il binding
        /// non funziona, per questa ragione in questo caso ammetto di inserire un ulteriore
        /// elemento in testa per indicare l'elemento nullo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="nullObject">The null object.</param>
        public static void AddRange<T>(
            this ObservableCollection<T> target,
            IEnumerable<T> source,
            T nullObject)
        {
            target.Add(nullObject);
            if (source != null)
            {
                foreach (T targetLinkDto in source)
                {
                    target.Add(targetLinkDto);
                }
            }
        }

        public static void AddRange<T>(
            this ObservableCollection<T> target,
            IEnumerable<T> source)
        {
            if (source != null)
            {
                foreach (T targetLinkDto in source)
                {
                    target.Add(targetLinkDto);
                }
            }
        }

        private static bool? _isInDesignMode;

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
            _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                        .FromProperty(prop, typeof(FrameworkElement))
                        .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }


    }
}
