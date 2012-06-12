using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;
using Login.Models;

namespace Login.Models
{
    //Test
    /// <summary>
    /// Die Klasse Bewerbung beschreibt eine Hiwi Bewerbung für ein Stellenangebot. 
    /// Dabei werden die Benutzerdaten mit einem Stellenangebot verknüpft.
    /// </summary>
    public class Bewerbung
    {
        [Integer]
        [Required]
        public int id { get; set; }

        //Stellenangebots ID zur eindeutigen referenzierung der Bewerbung an dein Stellenangebot
        [Integer]
        [Required]
        public int stellenangebotID { get; set; }


        //Benutzer ID zur eindeutigen referenzierung eines Hiwis für ein Stellenangebot
        [Integer]
        [Required]
        public int benutzerID { get; set; }

        //Hiwi Kenntnisse für die Bewerbung (Null werte sind zugelassen)
        public string kenntnisse { get; set; }


        //Status beschreibt den Bearbeitungsgrad der Bewerbung (0 = unbearbeitet, 1 = bewerbung abgelehnt, 2 = bewerbung angenommen & zur bearbeitung freigegeben)
        [Integer]
        [Required]
        public int status { get; set; }

        /**
            Benachrichtigung zeigt Nachrichten für den Bewerber bzw. den Anbieter an (0 = keine Nachricht, 1 = Bewerbung eingetroffen, 2 = neue Nachricht für Bewerber,
            3 = neue Nachricht für Anbieter)
        **/
        [Integer]
        [Required]
        public int benachrichtigung { get; set; }

        //Konversation zwischen Anbieter und Bewerber
        public string bemerkung { get; set; }
    }

    public class BewerbungAnsicht
    {
        public BewerbungAnsicht()
        {
            this.stelle = new Stellenangebot();
            this.anbieter = new Anbieter();
            this.bewerber = new Bewerber();
            this.bewerbung = new Bewerbung();
        }

        //Stellenangebot zur Bewerbung
        public Stellenangebot stelle { get; set;}

        //Anbieter der Stelle
        public Anbieter anbieter { get; set; }

        //Bewerber der sich auf die Stelle beworben hat
        public Bewerber bewerber { get; set; }

        public Bewerbung bewerbung { get; set; }

    }

    public class BewerbungDetails : Bewerbung
    {
        public BewerbungDetails()
        {
            this.stelle = new Stellenangebot();
        }

        public Stellenangebot stelle { get; set; }
    }

    public class BewerbungBenutzer
    {
        public Bewerber bewerber { get; set; }
        public Bewerbung bewerbung { get; set; }
    }
}