using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Login.Models;
using System.Web.Security;

namespace Login.Controllers
{
    public class BewerbungenController : Controller
    {
        //
        // GET: /AngeboteManager/
        DBManager DB = DBManager.getInstanz();

        /// <summary>
        /// ruft eine Bewerbung in der Uebersicht auf
        /// </summary>
        /// <param name="stelle"></param>
        /// <returns></returns>
        
        public ActionResult NeueBewerbung(Stellenangebot stelle)
        {
            Stellenangebot angebot = DB.stellenangebotLesen(stelle.id);
            string email = HttpContext.User.Identity.Name;
            Bewerber bewerber = DB.bewerberAuslesen(email);
            Anbieter anbieter = DB.anbieterAuslesen(angebot.anbieterID);
            BewerbungAnsicht bewerbung = new BewerbungAnsicht();
            bewerbung.stelle = angebot;
            bewerbung.bewerber = bewerber;
            bewerbung.anbieter = anbieter;
            Bewerbung temp = DB.bewerbungLesen(bewerber.id, angebot.id);

            
            temp.benachrichtigung = 0;

            DB.bewerbungAktualisieren(temp);

            if (temp != null)
            {
                bewerbung.bewerbung = temp;
            }

            //Überpfrüfen ob Bewerbung abgelehnt wurde, wird anschließend aus der Datenbank gelöscht
            if (bewerbung.bewerbung.status == 1)
            {
                DB.bewerbungLoeschen(bewerbung.bewerbung.id);
            }

            return View("Bewerbung", bewerbung);
        }

        [HttpPost]
        [Authorize(Roles = "Anbieter")]
        public ActionResult Bewerbung(int bewerbungID)
        {
            BewerbungAnsicht anzeige = new BewerbungAnsicht();
            anzeige.bewerbung = DB.bewerbungLesen(bewerbungID);
            anzeige.bewerber = DB.bewerberAuslesen(anzeige.bewerbung.benutzerID);
            anzeige.stelle = DB.stellenangebotLesen(anzeige.bewerbung.stellenangebotID);
            anzeige.anbieter = DB.anbieterAuslesen(anzeige.stelle.anbieterID);

            Bewerbung aktuell = anzeige.bewerbung;
            aktuell.benachrichtigung = 0;

            DB.bewerbungAktualisieren(aktuell);

            return View("Bewerbung", anzeige);
        }

        /// <summary>
        /// Bewerbung wird hinzugefügt oder aktualisiert
        /// </summary>
        /// <param name="bewerbungBenutzer"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BewerbungSpeichern(BewerbungAnsicht bewerbungBenutzer, string bemerkungNeu)
        {
            Bewerbung bewerbung = new Bewerbung();
            bewerbung.stellenangebotID = bewerbungBenutzer.stelle.id;
            bewerbung.benutzerID = bewerbungBenutzer.bewerber.id;
            bewerbung.kenntnisse = bewerbungBenutzer.bewerbung.kenntnisse;
            string email = HttpContext.User.Identity.Name;
            Benutzer benutzer = DB.benutzerAuslesen(email);
            

            if (DB.bewerbungVorhanden(bewerbung.benutzerID, bewerbung.stellenangebotID) == false)
            {
                bewerbung.status = 0;
                bewerbung.benachrichtigung = 1;
                DB.bewerbungHinzufügen(bewerbung);
            }
            else
            {
                bewerbung.id = bewerbungBenutzer.bewerbung.id;
                bewerbung.status = bewerbungBenutzer.bewerbung.status;
                bewerbung.kenntnisse = bewerbungBenutzer.bewerbung.kenntnisse;
                bewerbung.bemerkung = bewerbungBenutzer.bewerbung.bemerkung;

                if (Roles.GetRolesForUser(email)[0].Equals("Bewerber"))
                {
                    bewerbung.bemerkung += "  Bewerber: " + benutzer.vorname + " " + benutzer.nachname + " schrieb am " + DateTime.Now.ToString() + " : " + bemerkungNeu;
                    bewerbung.benachrichtigung = 3;
                }
                else if (Roles.GetRolesForUser(email)[0].Equals("Anbieter"))
                {
                    bewerbung.bemerkung += "  Anbieter: " + benutzer.vorname + " " + benutzer.nachname + " schrieb am " + DateTime.Now.ToString() + " : " + bemerkungNeu;
                    bewerbung.benachrichtigung = 2;
                }
                
                DB.bewerbungAktualisieren(bewerbung);
            }

            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Anbieter")]
        public ActionResult BewerbungAblehnen(int id)
        {
            Bewerbung bewerbungAkt = DB.bewerbungLesen(id);
            Stellenangebot stelle = DB.stellenangebotLesen(bewerbungAkt.stellenangebotID);
            bewerbungAkt.status = 1;
            DB.bewerbungAktualisieren(bewerbungAkt);
            return View("StellenAngebot", "AngeboteManager", stelle);
        }

        [Authorize(Roles = "Anbieter")]
        public ActionResult BewerbungAnnehmen(int id)
        {
            Bewerbung bewerbungAkt = DB.bewerbungLesen(id);
            Stellenangebot stelle = DB.stellenangebotLesen(bewerbungAkt.stellenangebotID);
            bewerbungAkt.status = 2;
            DB.bewerbungAktualisieren(bewerbungAkt);
            return View("StellenAngebot", "AngeboteManager", stelle);
        }

        public ActionResult BewerbungLoeschen(BewerbungAnsicht bewerbungBenutzer)
        {
            DB.bewerbungLoeschen(bewerbungBenutzer.bewerbung.id);
            return RedirectToAction("Index", "User");
        }

        /// <summary>
        /// Läd alle eigenen Stellenangebote in eine Liste und verknüpft sie mit den zugehörigen
        /// Bewerbungen. Fügt sie der Partiellen View _StellenangeboteÜbersicht hinzu
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Anbieter")]
        public PartialViewResult _AngebotBewerbungenSteuerung()
        {
            string email = HttpContext.User.Identity.Name;
            Anbieter benutzer = DB.anbieterAuslesen(email);



            LinkedList<StellenangebotAnsicht> angebotBewerber = DB.stellenangeboteUebersichtLesen(benutzer.id);
            

            for (int i = 0; i < angebotBewerber.Count; i++)
            {
                LinkedList<BewerbungBenutzer> bewerbungBenutzer = new LinkedList<BewerbungBenutzer>();
                LinkedList<Bewerbung> bewerbungen = DB.bewerbungenLesenVonAnbieter(angebotBewerber.ElementAt(i).id);

                for (int j = 0; j < bewerbungen.Count; j++)
                {
                    BewerbungBenutzer temp = new BewerbungBenutzer();
                    temp.bewerber = DB.bewerberAuslesen(bewerbungen.ElementAt(j).benutzerID);
                    temp.bewerbung = bewerbungen.ElementAt(j);

                    bewerbungBenutzer.AddLast(temp);
                }
                angebotBewerber.ElementAt(i).bewerbungen = bewerbungBenutzer;
            }
            StellenangeboteBewerbungUebersicht model = new StellenangeboteBewerbungUebersicht(angebotBewerber);

            return PartialView(model);

        }

        /// <summary>
        /// Lädt die Bewerbungen eines Benutzers
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Bewerber")]
        public PartialViewResult _BewerbungenSteuerung()
        {
            string email = HttpContext.User.Identity.Name;
            Bewerber benutzer = DB.bewerberAuslesen(email);
            LinkedList<Bewerbung> bewerbungen = DB.bewerbungenUebersichtLesen(benutzer.id);
            LinkedList<BewerbungDetails> liste = new LinkedList<BewerbungDetails>();

            for (int i = 0; i < bewerbungen.Count; i++)
            {
                BewerbungDetails temp = new BewerbungDetails();
                temp.id = bewerbungen.ElementAt(i).id;
                temp.benutzerID = bewerbungen.ElementAt(i).benutzerID;
                temp.kenntnisse = bewerbungen.ElementAt(i).kenntnisse;
                temp.status = bewerbungen.ElementAt(i).status;
                temp.benachrichtigung = bewerbungen.ElementAt(i).benachrichtigung;

                temp.stelle = DB.stellenangebotLesen(bewerbungen.ElementAt(i).stellenangebotID);

                liste.AddLast(temp);
            }

            BewerbungenUebersicht bewerbungenUebersicht = new BewerbungenUebersicht(liste);
            return PartialView(bewerbungenUebersicht);

        }

        /// <summary>
        /// Lädt alle Bewerbungen eines Stellenangebots
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Anbieter")]
        public PartialViewResult _BewerbungenStellenangebot(int stellenangebotID)
        {
            LinkedList<Bewerbung> bewerbungen = DB.bewerbungenLesenVonAnbieter(stellenangebotID);
            LinkedList<BewerbungBenutzer> bewerbungenBenutzer = new LinkedList<BewerbungBenutzer>();
            for (int i = 0; i < bewerbungen.Count; i++)
            {
                BewerbungBenutzer temp = new BewerbungBenutzer();
                temp.bewerbung = bewerbungen.ElementAt(i);
                temp.bewerber = DB.bewerberAuslesen(bewerbungen.ElementAt(i).benutzerID);
                bewerbungenBenutzer.AddLast(temp);
            }
            StellenangebotAnsicht ansicht = new StellenangebotAnsicht(bewerbungenBenutzer);
            return PartialView(ansicht);
        }
    }
}
