#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using SchoolEscape.Puzzles;

namespace SchoolEscape.EditorTools {
    /// <summary>
    /// Menu: School Escape > Create Sample Dutch Puzzles.
    /// Creates ready-to-use groep 7/8 puzzles as ScriptableObjects. Existing assets are never overwritten.
    /// </summary>
    public static class SamplePuzzleCreator {
        private const string Folder = "Assets/_Project/ScriptableObjects/Puzzles";

        [MenuItem("School Escape/Create Sample Dutch Puzzles")]
        public static void CreateAll() {
            EnsureFolders();
            int created = 0, skipped = 0;

            Make("Puzzle_TaalWerkwoorden", ref created, ref skipped, d => {
                d.EditorInitialize("taal_werkwoorden", "Werkwoorden", "TAALCOMPUTER - WERKWOORDEN", PuzzleCategory.DutchLanguage,
                    PuzzleDifficulty.Medium, PuzzleSkin.Computer, "Beantwoord de vragen over werkwoorden en zinnen.",
                    "Computer gebruiken", "Goed gedaan! Je hebt de opdracht voltooid.",
                    new PuzzleReward(PuzzleRewardKind.InventoryItem, "klaslokaal_sleutel", "Je hebt de sleutel van het klaslokaal gevonden!"));
                d.EditorSetQuestions(new[]
                {
                    QuestionData.ChoiceQuestion("Welke zin is correct geschreven?", 1,
                        "Bij 'ik' laat je de -t weg: ik word.", "Denk aan de regel: bij 'ik' komt er geen -t achter.",
                        "Ik wordt morgen tien jaar.", "Ik word morgen tien jaar.", "Ik wort morgen tien jaar."),
                    QuestionData.TextQuestion("Wat is het voltooid deelwoord van 'lopen'?",
                        "Je schrijft: ik heb gelopen.", "Begin met 'ge-'.", "gelopen"),
                    QuestionData.TextQuestion("Wat is de persoonsvorm in deze zin?\n\nDe kinderen spelen buiten voetbal.",
                        "In de verleden tijd wordt 'spelen' 'speelden'. Dat woord verandert, dus het is de persoonsvorm.",
                        "Zet de zin in de verleden tijd. Welk woord verandert?", "spelen"),
                    QuestionData.ChoiceQuestion("Wat is het tegenovergestelde van 'voorzichtig'?", 0,
                        "Onvoorzichtig is het tegenovergestelde. 'Behoedzaam' betekent juist hetzelfde als voorzichtig.", "",
                        "onvoorzichtig", "behoedzaam", "rustig", "zorgvuldig"),
                    QuestionData.TextQuestion("Vul de verleden tijd in:\n\nGisteren (fietsen) wij naar school.",
                        "De stam is 'fiets' en eindigt op een 's' (het staat in 't kofschip). Dus: -ten, fietsten.",
                        "Gebruik 't kofschip.", "fietsten"),
                    QuestionData.TextQuestion("Wat is het voltooid deelwoord van 'schrijven'?",
                        "Je schrijft: ik heb geschreven.", "", "geschreven"),
                });
            });

            Make("Puzzle_RekenenGroep8", ref created, ref skipped, d => {
                d.EditorInitialize("rekenen_groep8", "Rekenen", "REKENCOMPUTER - GROEP 8", PuzzleCategory.Mathematics,
                    PuzzleDifficulty.Medium, PuzzleSkin.Computer, "Los de volgende sommen op.",
                    "Computer gebruiken", "Goed gedaan! Je hebt de opdracht voltooid.",
                    new PuzzleReward(PuzzleRewardKind.ProgressionFlag, "computer_password_known", "Op het scherm verschijnt een geheim wachtwoord!"));
                d.EditorSetQuestions(new[]
                {
                    QuestionData.NumberQuestion("125 x 8 = ?", 1000, "100 x 8 = 800 en 25 x 8 = 200. Samen is dat 1000.", "Reken 100 x 8 en 25 x 8 apart uit."),
                    QuestionData.NumberQuestion("144 : 12 = ?", 12, "12 x 12 = 144.", ""),
                    QuestionData.NumberQuestion("3/4 + 1/4 = ?", 1, "Vier kwart is een hele.", ""),
                    QuestionData.NumberQuestion("Hoeveel is 25% van 200?", 50, "25% is een kwart. Een kwart van 200 is 50.", "25% is hetzelfde als een kwart."),
                    QuestionData.NumberQuestion("Een winkel verkoopt een spel voor \u20ac40. Je krijgt 25% korting. Hoeveel euro betaal je?", 30,
                        "25% van 40 is 10. Dus 40 - 10 = 30 euro.", "Reken eerst de korting uit.", "Antwoord in euro"),
                    QuestionData.NumberQuestion("Hoeveel centimeter is 2,5 meter?", 250, "1 meter is 100 cm, dus 2,5 x 100 = 250 cm.", "", "Antwoord in cm"),
                    QuestionData.TextQuestion("De trein vertrekt om 14:35 en rijdt 50 minuten.\nHoe laat komt hij aan? (Schrijf het zo: 09:05)",
                        "14:35 + 25 minuten = 15:00, en dan nog 25 minuten. Dus 15:25.", "Tel eerst bij tot het hele uur.",
                        "15:25", "15.25", "15:25 uur", "15.25 uur"),
                });
            });

            Make("Puzzle_AardrijkskundeEuropa", ref created, ref skipped, d => {
                d.EditorInitialize("aardrijkskunde_europa", "Aardrijkskunde", "AARDRIJKSKUNDE-BORD - GROEP 7", PuzzleCategory.Geography,
                    PuzzleDifficulty.Easy, PuzzleSkin.Blackboard, "Kies steeds het juiste antwoord.",
                    "Bord bekijken", "Goed gedaan! Je weet veel over de wereld.",
                    new PuzzleReward(PuzzleRewardKind.ProgressionFlag, "bord_opgelost", "Er klikt iets in de muur..."));
                d.EditorSetQuestions(new[]
                {
                    QuestionData.ChoiceQuestion("Wat is de hoofdstad van België?", 0, "Brussel is de hoofdstad van België.", "", "Brussel", "Antwerpen", "Gent", "Luik"),
                    QuestionData.ChoiceQuestion("Wat is de hoofdstad van de provincie Zeeland?", 0, "Middelburg is de hoofdstad van Zeeland.", "", "Middelburg", "Vlissingen", "Goes", "Terneuzen"),
                    QuestionData.ChoiceQuestion("Hoeveel provincies heeft Nederland?", 1, "Nederland heeft 12 provincies.", "", "10", "12", "14", "16"),
                    QuestionData.ChoiceQuestion("Door welke stad stroomt de rivier de Seine?", 2, "De Seine stroomt door Parijs.", "", "Londen", "Madrid", "Parijs", "Berlijn"),
                    QuestionData.ChoiceQuestion("Op welk continent ligt Kenia?", 1, "Kenia ligt in Afrika.", "", "Azië", "Afrika", "Europa", "Zuid-Amerika"),
                    QuestionData.TextQuestion("Hoe heet de hoofdstad van Duitsland?", "Berlijn is de hoofdstad van Duitsland.", "Het begint met een B.", "Berlijn"),
                });
            });

            Make("Puzzle_DigitaalVeilig", ref created, ref skipped, d => {
                d.EditorInitialize("digitaal_veilig", "Veilig online", "VEILIG ONLINE - GROEP 8", PuzzleCategory.DigitalLiteracy,
                    PuzzleDifficulty.Medium, PuzzleSkin.Paper, "Hoe gedraag jij je veilig op internet?",
                    "Opdracht lezen", "Goed gedaan! Jij weet hoe je veilig online bent.",
                    new PuzzleReward(PuzzleRewardKind.InventoryItem, "bibliotheek_sleutel", "Je hebt de sleutel van de bibliotheek gevonden!"));
                d.EditorSetQuestions(new[]
                {
                    QuestionData.ChoiceQuestion("Welk wachtwoord is het veiligst?", 2,
                        "Een lang wachtwoord met hoofdletters, cijfers en tekens is moeilijk te raden.", "Kies een wachtwoord dat lang en moeilijk te raden is.",
                        "Fluffy2012", "mijnnaam", "Tr0mpet!Vlinder#42", "12345678"),
                    QuestionData.ChoiceQuestion("Je krijgt een bericht: 'Gefeliciteerd! Je hebt een gratis iPad gewonnen. Klik snel op de link!' Wat doe je?", 2,
                        "Zulke berichten zijn vaak oplichting. Klik niet en vraag een volwassene om hulp.", "",
                        "Ik klik op de link", "Ik stuur het naar al mijn vrienden", "Ik klik niet en laat het aan een volwassene zien", "Ik vul mijn wachtwoord in"),
                    QuestionData.ChoiceQuestion("Mag je je wachtwoord aan een vriend geven?", 1,
                        "Een wachtwoord is geheim. Je deelt het alleen met je ouders of verzorgers.", "",
                        "Ja, als het mijn beste vriend is", "Nee, je houdt je wachtwoord geheim", "Ja, als hij belooft het niet door te vertellen", "Ja, dan kunnen we samen gamen"),
                    QuestionData.ChoiceQuestion("Welke bron is waarschijnlijk het betrouwbaarst voor een werkstuk over dieren?", 2,
                        "Een bekend museum of een natuurorganisatie controleert zijn informatie.", "Wie weet echt veel over dieren?",
                        "Een anoniem blog zonder bronnen", "Een filmpje van een onbekende", "De website van een natuurmuseum", "Een bericht in een groepsapp"),
                });
            });

            Make("Puzzle_NatuurWereld", ref created, ref skipped, d => {
                d.EditorInitialize("natuur_wereld", "Natuur en wereld", "NATUURSTATION - GROEP 7/8", PuzzleCategory.Science,
                    PuzzleDifficulty.Medium, PuzzleSkin.Keypad, "Test je kennis over de natuur.",
                    "Station gebruiken", "Goed gedaan! Het slot gaat open.",
                    new PuzzleReward(PuzzleRewardKind.ProgressionFlag, "natuurslot_open", "Het slot gaat open!"));
                d.EditorSetQuestions(new[]
                {
                    QuestionData.ChoiceQuestion("Welk orgaan pompt het bloed door je lichaam?", 0, "Het hart pompt het bloed rond.", "", "Het hart", "De longen", "De maag", "De nieren"),
                    QuestionData.ChoiceQuestion("Welke planeet staat het dichtst bij de zon?", 0, "Mercurius staat het dichtst bij de zon.", "", "Mercurius", "Venus", "Mars", "Aarde"),
                    QuestionData.TextQuestion("Welk gas nemen planten op uit de lucht om te groeien (fotosynthese)?",
                        "Planten nemen koolstofdioxide op en maken er suiker en zuurstof van.", "Het is het gas dat wij uitademen.",
                        "koolstofdioxide", "kooldioxide", "co2"),
                    QuestionData.NumberQuestion("Hoeveel planeten heeft ons zonnestelsel?", 8, "Mercurius, Venus, Aarde, Mars, Jupiter, Saturnus, Uranus en Neptunus.", ""),
                    QuestionData.ChoiceQuestion("Welke energiebron raakt nooit op?", 2, "De zon blijft schijnen. Olie, gas en steenkool raken wel op.", "",
                        "Aardgas", "Steenkool", "Zonne-energie", "Olie"),
                    QuestionData.ChoiceQuestion("In welke container hoort een lege glazen jampot?", 1, "Glas gaat in de glasbak, zodat het kan worden hergebruikt.", "",
                        "Papiercontainer", "Glasbak", "Restafval", "GFT-bak"),
                });
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Voorbeeldopdrachten", $"Created: {created}\nSkipped (already exist): {skipped}\n\nLocation: {Folder}", "OK");
        }

        private static void Make(string assetName, ref int created, ref int skipped, Action<QuestionPuzzleDefinition> setup) {
            string path = $"{Folder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<QuestionPuzzleDefinition>(path) != null) { skipped++; return; }

            var def = ScriptableObject.CreateInstance<QuestionPuzzleDefinition>();
            setup(def);
            AssetDatabase.CreateAsset(def, path);
            EditorUtility.SetDirty(def);
            created++;
        }

        private static void EnsureFolders() {
            string[] parts = { "Assets", "_Project", "ScriptableObjects", "Puzzles" };
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif