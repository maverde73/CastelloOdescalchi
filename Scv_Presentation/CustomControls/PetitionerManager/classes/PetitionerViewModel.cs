using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;
using Scv_Entities;
using System.ComponentModel;

namespace Presentation
{

    public class PetitionerViewModel : INotifyPropertyChanged
    {

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // Events


        #region Properties

        private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

        private ObservableCollection<Richiedente> availables = null;
        public ObservableCollection<Richiedente> Availables
        {
            get
            {
                if (availables == null)
                    availables = new ObservableCollection<Richiedente>();
                return availables;
            }
            set { availables = value; }
        }

        private ObservableCollection<PetitionerSearchItem> searchItems = null;
        public ObservableCollection<PetitionerSearchItem> SearchItems
        {
            get
            {
                if (searchItems == null)
                    searchItems = new ObservableCollection<PetitionerSearchItem>();
                return searchItems;
            }
            set { searchItems = value; }
        }

        private Richiedente selectedItem = null;
        public Richiedente SelectedItem
        {
            get
            {
                if (selectedItem == null)
                    selectedItem = new Richiedente();
                return selectedItem;
            }
            set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
        }

        private Richiedente autoCompleteBoxSelectedItem = null;
        public Richiedente AutoCompleteBoxSelectedItem
        {
            get { return autoCompleteBoxSelectedItem; }
            set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
        }

        #endregion // Properties


        #region Contstructors

        public PetitionerViewModel()
        {
            if (!designTime)
            {
                Availables = LoadAvailables();
                foreach (Richiedente r in Availables)
                {
                    PetitionerSearchItem si = new PetitionerSearchItem();
                    si.ID = r.Id_Richiedente;
                    if (!string.IsNullOrEmpty(r.Nome))
                        si.FirstName = r.Nome;
                    else
                        si.FirstName = "";
                    si.LastName = r.Cognome;
                    SearchItems.Add(si);
                }
            }


        }

        #endregion // Constructors


        #region Event handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            switch (propertyName)
            {
                case "AutoCompleteBoxSelectedItem":
                    if (AutoCompleteBoxSelectedItem != null)
                        SelectedItem = Clone(autoCompleteBoxSelectedItem);
                    break;
            }

            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // Event handling


        #region Main Methods

        public void SelectItem(int itemID)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Richiedente r = null;

                try
                {
                    r = (Richiedente)_context.Richiedenti.First(x => x.Id_Richiedente == itemID);
                }
                catch (Exception e)
                {

                }
                SelectedItem = r;
                AutoCompleteBoxSelectedItem = r;
            }
        }

        private ObservableCollection<Richiedente> LoadAvailables()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return new ObservableCollection<Richiedente>(_context.Richiedenti);
            }
        }

        private Richiedente Clone(Richiedente source)
        {
            Richiedente destination = new Richiedente();

            ((Richiedente)destination).Cognome = ((Richiedente)source).Cognome;
            ((Richiedente)destination).Nome = ((Richiedente)source).Nome;
            ((Richiedente)destination).Id_Titolo = ((Richiedente)source).Id_Titolo;
            ((Richiedente)destination).Id_Organizzazione = ((Richiedente)source).Id_Organizzazione;
            ((Richiedente)destination).Id_LinguaAbituale = ((Richiedente)source).Id_LinguaAbituale;
            ((Richiedente)destination).Id_Citta = ((Richiedente)source).Id_Citta;
            ((Richiedente)destination).Tel_Casa = ((Richiedente)source).Tel_Casa;
            ((Richiedente)destination).Tel_Ufficio = ((Richiedente)source).Tel_Ufficio;
            ((Richiedente)destination).Tel_Cellulare = ((Richiedente)source).Tel_Cellulare;
            ((Richiedente)destination).Email = ((Richiedente)source).Email;
            ((Richiedente)destination).Indirizzo = ((Richiedente)source).Indirizzo;
            ((Richiedente)destination).Nota = ((Richiedente)source).Nota;
            ((Richiedente)destination).Dt_Update = DateTime.Now;

            return destination;
        }

        public void GetItemByText(object sender, DoWorkEventArgs e)
        {
            string text = e.Argument.ToString();
            using (IN_VIAEntities cnt = new IN_VIAEntities())
            {
                try
                {
                    Richiedente p = cnt.Richiedenti.FirstOrDefault(x => String.Equals(x.Cognome, text));
                    e.Result = p;
                }
                catch (Exception ex)
                {

                }
            }
        }

        #endregion
    }
}