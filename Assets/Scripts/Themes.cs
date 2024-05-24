using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Themes : MonoBehaviour
{
public List<string> themes = new List<string>()
{
    "Abuse of power",
    "Adultery",
    "Adversity",
    "Aging",
    "Alienation",
    "Ambitions",
    "American dream",
    "Arrogance",
    "Art",
    "Autonomy",
    "Beauty",
    "Beliefs",
    "Betrayal",
    "Bravery",
    "Capitalism",
    "Celebration",
    "Chance",
    "Change versus tradition",
    "Chaos and order",
    "Character",
    "Childhood",
    "Circle of life",
    "Class",
    "Climate change",
    "Colonialism",
    "Coming of age",
    "Common sense",
    "Communication",
    "Companionship",
    "Conservation",
    "Conspiracy",
    "Convention and rebellion",
    "Corruption",
    "Courage",
    "Creation",
    "Crime",
    "Darkness and light",
    "Death",
    "Dedication",
    "Democracy",
    "Depression",
    "Desire",
    "Despair",
    "Destiny",
    "Disappointment",
    "Disillusionment",
    "Displacement",
    "Dreams",
    "Economics",
    "Education",
    "Empowerment",
    "Everlasting love",
    "Failure",
    "Faith",
    "Fame",
    "Family",
    "Fate",
    "Fear",
    "Feminism",
    "Forbidden love",
    "Forgiveness",
    "Free will",
    "Freedom",
    "Friendship",
    "Fulfillment",
    "Future",
    "Gender",
    "God",
    "Good vs evil",
    "Government",
    "Gratitude",
    "Greed",
    "Growing up",
    "Guilt",
    "Happiness",
    "Hard work",
    "Hate",
    "Health",
    "Heartbreak",
    "Hero",
    "Heroism",
    "History",
    "Honesty",
    "Honor",
    "Hope",
    "Humankind",
    "Human nature",
    "Humility",
    "Humor",
    "Hypocrisy",
    "Identity",
    "Ideology",
    "Imagination",
    "Immortality",
    "Imperialism",
    "Impossibility",
    "Individuality",
    "Inequality",
    "Injustice",
    "Innocence",
    "Inspiration",
    "Isolation",
    "Jealousy",
    "Joy",
    "Justice",
    "Kindness",
    "Knowledge",
    "Law",
    "Legacy",
    "LGBTQ+ rights",
    "Life",
    "Loneliness",
    "Loss",
    "Love",
    "Loyalty",
    "Madness",
    "Manipulation",
    "Materialism",
    "Maturity",
    "Medicine",
    "Memories",
    "Mercy",
    "Money",
    "Morality",
    "Motherhood",
    "Music",
    "Nationalism",
    "Nature",
    "Necessity",
    "Neglect",
    "New year",
    "Normality",
    "Not giving up",
    "Oneness",
    "Opportunity",
    "Oppression",
    "Optimism",
    "Overcoming",
    "Passion",
    "Peace",
    "Peer pressure",
    "Perfection",
    "Perseverance",
    "Personal development",
    "Politics",
    "Poverty",
    "Power",
    "Prayer",
    "Prejudice",
    "Pride",
    "Progress",
    "Propaganda",
    "Purpose",
    "Race",
    "Realism",
    "Reality",
    "Rebellion",
    "Rebirth",
    "Redemption",
    "Regret",
    "Relationship",
    "Religion",
    "Repression",
    "Resistance",
    "Revenge",
    "Revolution",
    "Sacrifice",
    "Sadness",
    "Satire",
    "Science",
    "Self-awareness",
    "Self-discipline",
    "Self-reliance",
    "Self-preservation",
    "Simplicity",
    "Sin",
    "Society",
    "Solitude",
    "Stoicism",
    "Subjectivity",
    "Suffering",
    "Suicide",
    "Surveillance",
    "Survival",
    "Sympathy",
    "Technology",
    "Temptation",
    "Time",
    "Tolerance",
    "Totalitarianism",
    "Tragedy",
    "Travel",
    "Trust",
    "Truth",
    "Unconditional love",
    "Universe",
    "Unrequited love",
    "Unselfishness",
    "Value",
    "Vanity",
    "Vices",
    "Violence",
    "Virtue",
    "War",
    "Waste",
    "Wealth",
    "Willpower",
    "Winning and losing",
    "Wisdom",
    "Work",
    "Working class struggles",
    "Xenophobia",
    "Youth"
};

public TMP_Dropdown    TMPDropdown;
public TMP_Text        text;
 
    void Start()
    {
        TMPDropdown.options.Clear ();
     foreach (string t in themes)
     {
         TMPDropdown.options.Add (new TMP_Dropdown.OptionData() {text=t});
     }
    }

}
