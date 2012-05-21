using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Login.Models
{
    public class BewerbungenUebersicht
    {
        public LinkedList<BewerbungDetails> bewerbungen { get; set; }

        public BewerbungenUebersicht(LinkedList<BewerbungDetails> _bewerbungen)
        {
            this.bewerbungen = _bewerbungen;
        }

        public BewerbungenUebersicht()
        {
            bewerbungen = null;
        }
    }
}