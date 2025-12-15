using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerTags
{
    public enum Tags {
        ArmourPiercing, 
        AntiAir, 
        ColdHearted
    }
}

public class TowerBehaviour : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public Dialogue(string Speaker, string text)
        {
            speaker = Speaker;
            dialogue = text;
        }
        public string speaker;
        public string dialogue;
    }

    public class DialogueScene
    {
        public DialogueScene(Dialogue[] Lines, int Answer)
        {
            lines = Lines;
            answer = Answer;
        }
        public Dialogue[] lines;
        public int answer;
    }

    [System.Serializable]
    public class TowerDialogue
    {
        public TowerDialogue(DialogueScene[] DI)
        {
            DialogueInteractions = DI;
        }
        public DialogueScene[] DialogueInteractions;
    }

    public string TowerName;

    public TowerDialogue GunnerDialogue;
    public TowerDialogue FlamethrowerDialogue;
    public TowerDialogue LaserDialogue;
    public TowerDialogue MissileDialogue;

    public LayerMask EnemiesLayer;
    public TextAsset JSON;
    public TowerDialogue towerDialogue;

    public PlayerStats playerStats;
    public EnemyStats Target;
    public TowerTargeting.TargetType TargetType;
    public Transform TowerPivot;
    public Canvas UpgradeMenu;
    public bool Menu;

    public int SummonCost = 100;
    public float Damage;
    public float Firerate;
    public float Range;

    public List<TowerTags.Tags> Tags;

    public int ExpNeeded;
    public int Exp;
    public int level;

    public Canvas NovelUI;
    public TextMeshProUGUI NameBox;
    public TextMeshProUGUI DialogueBox;

     public bool talking;
    public int D_index = 0;
    public int S_index = 0;

    public bool Upgrade1;
    public bool Upgrade2;
    public bool Upgrade3;

    public Button FlirtButton;
    public Button UpgradeButton1;
    public Button UpgradeButton2;
    public Button UpgradeButton3;

    public GameObject Heart1;
    public GameObject Heart2;
    public GameObject Heart3;

    public GameObject lock1;
    public GameObject lock2;
    public GameObject lock3;

    private float Delay; // time before apply damage

    private IDamageMethod CurrentDamageMethodClass;

    /* 
    *   *** THIS IS FOR TESTING; REMOVE WHEN NEEDED *** 
    */
    // bool first1 = true;
    // bool first2 = true;
    // bool first3 = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDialogue();

        if (TowerName == "Gunner")
            towerDialogue = GunnerDialogue;
        else if (TowerName == "Flamethrower")
            towerDialogue = FlamethrowerDialogue;
        else if (TowerName == "Laser")
            towerDialogue = LaserDialogue;
        else if (TowerName == "Missile")
            towerDialogue = MissileDialogue;

        CurrentDamageMethodClass = GetComponent<IDamageMethod>();
        Tags = new List<TowerTags.Tags>();

        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();

        //towerDialogue = new TowerDialogue();
        //towerDialogue = JsonUtility.FromJson<TowerDialogue>(JSON.text);

        Menu = false;
        UpgradeMenu.enabled = false;
        UpgradeMenu.gameObject.SetActive(false);
        FlirtButton.interactable = false;

        ExpNeeded = 1;
        Exp = 0;
        level = 0;
        talking = false;
        S_index = 0;

        Upgrade1 = false;
        Upgrade2 = false;
        Upgrade3 = false;

        if (CurrentDamageMethodClass == null)
        {
            Debug.LogError("TOWERS: No damage class attached to given tower!");
        }
        else 
        {
            CurrentDamageMethodClass.Init(Damage, Firerate);
        }
        Delay = 1 / Firerate;
    }

    // Update is called once per frame
    public void Tick()
    {

        CurrentDamageMethodClass.DamageTick(Target);

        if (Target != null)
        {
            TowerPivot.transform.rotation = Quaternion.LookRotation(Target.transform.position - transform.position);
        }
    }

    public void Update()
    {
        /* --- THIS IS FOR TESTING PURPOSES --- */
        // if (Upgrade1 && first1)
        // {
        //     first1 = false;
        //     FlamethrowerUpgrade1();
        // }
        // if (Upgrade2 && first2)
        // {
        //     first2 = false;
        //     FlamethrowerUpgrade2();
        // }
        // if (Upgrade3 && first3)
        // {
        //     first3 = false;
        //     FlamethrowerUpgrade3();
        // }

        if (level >= 1)
        {
            lock1.SetActive(false);
        }

        if (Upgrade1)
        {
            UpgradeButton1.interactable = false;

            if (level >= 2)
                lock2.SetActive(false);
        
        }

        if (Upgrade2)
        {
            UpgradeButton2.interactable = false;

            if (level >= 3)
                lock3.SetActive(false);
        }
        
        if (Upgrade3)
        {
            UpgradeButton3.interactable = false;
        }


        if (talking) 
        {
            if (D_index < towerDialogue.DialogueInteractions[S_index].lines.Length - 1)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    D_index += 1;
                    DisplayDialogue();
                }
            }
            else
            {
                bool flag1 = Input.GetKey(KeyCode.Alpha1);
                bool flag2 = Input.GetKey(KeyCode.Alpha2);
                bool flag3 = Input.GetKey(KeyCode.Alpha3);

                int answer = towerDialogue.DialogueInteractions[S_index].answer;

                if (flag1 || flag2 || flag3)
                {
                    Debug.Log("1: " + flag1 + "\n2: " + flag2 + "\n3: " + flag3);

                    if ((flag1 && answer == 1) || (flag2 && answer == 2) || (flag3 && answer == 3))
                        IncreaseRelationship();
                        S_index += 1;

                    NovelUI.enabled = false;
                    NovelUI.gameObject.SetActive(false);
                    talking = false;
                    playerStats.IsTalking(false);
                }
            }
        }
    }

    public void ToggleMenu()
    {
        Menu = !Menu;
        UpgradeMenu.enabled = Menu;
        UpgradeMenu.gameObject.SetActive(Menu);
    }

    public void ResetDialogue()
    {
        FlirtButton.interactable = true;
    }

    public void StartDialogue()
    {
        talking = true;
        playerStats.IsTalking(true);

        NovelUI.enabled = true;
        NovelUI.gameObject.SetActive(true);
        FlirtButton.interactable = false;

        D_index = 0;
        DisplayDialogue();
    }

    public void DisplayDialogue()
    {
        NameBox.text = towerDialogue.DialogueInteractions[S_index].lines[D_index].speaker;
        DialogueBox.text = towerDialogue.DialogueInteractions[S_index].lines[D_index].dialogue;
    }

    public void IncreaseRelationship()
    {
        Exp += 1;

        if (Exp >= ExpNeeded)
        {
            Exp = 0;
            //ExpNeeded += 1;
            level += 1;
        }

        if (level == 1)
        {
            Image Heart = Heart1.GetComponent<Image>();
            Heart.color = Color.red;
        }

        if (level == 2)
        {
            Image Heart = Heart2.GetComponent<Image>();
            Heart.color = Color.red;
        }

        if (level == 3)
        {
            Image Heart = Heart3.GetComponent<Image>();
            Heart.color = Color.red;
            FlirtButton.interactable = false;
        }
    }

    public void CreateDialogue()
    {
        Dialogue l1 = new Dialogue("Gunner", "Hey Mami, Lemme whisper in your ear. Gunner Down's the name, and seducing you is my game.");
        Dialogue l2 = new Dialogue("Player", "... The F**K?!");
        Dialogue l3 = new Dialogue("Gunner", "Sorry Sweetheart, didn't mean to startle you like that, but what can I say? You blew my mind.");
        Dialogue l4 = new Dialogue("Player", "Are you always this forward?");
        Dialogue l5 = new Dialogue("Gunner", "I see what I want and I take it. Can't handle my heat? Then get out the kitchen");
        Dialogue l6 = new Dialogue("Player", "1. Yes Chef!\n2. Uh... OK?\n3. I love cooking actually!");

        DialogueScene ds1 = new DialogueScene(new Dialogue[]{l1, l2, l3, l4, l5, l6}, 1);

        Dialogue l7 = new Dialogue("Gunner", "Knock Knock.");
        Dialogue l8 = new Dialogue("Player", "Who's there?");
        Dialogue l9 = new Dialogue("Gunner", "The love of your life, if we stopped playing games...");
        Dialogue l10 = new Dialogue("Player", "1. This is private propery, I'm gonna have to ask you to leave...\n2. Tempting offer, but I'm a slow-burn fan.\n3. Dude, we are literally STILL AT WAR");

        DialogueScene ds2 = new DialogueScene(new Dialogue[]{l7, l8, l9, l10}, 2);

        Dialogue l11 = new Dialogue("Gunner", "Hey Commander?");
        Dialogue l12 = new Dialogue("Player", "Yes Gunner.");
        Dialogue l13 = new Dialogue("Gunner", "I feel we've really gotten to know each other. I appreciate that.");
        Dialogue l14 = new Dialogue("Player", "Well, you are our best shooter here. Who else is gonna deal with our high priority targets?");
        Dialogue l15 = new Dialogue("Gunner", "Oh don't you worry, this shooter knows exactly who his priorities are.");
        Dialogue l16 = new Dialogue("Gunner", "You just don't forget about me when the war ends, alright?");
        Dialogue l17 = new Dialogue("Player", "1. Uh... sure?\n2. Well I'm sure as hell going to try.\n3. Wouldn't dream of it, Handsome.");

        DialogueScene ds3 = new DialogueScene(new Dialogue[]{l11, l12, l13, l14, l15, l16, l17}, 3);

        GunnerDialogue = new TowerDialogue(new DialogueScene[]{ds1, ds2, ds3});

        Dialogue l18 = new Dialogue("Player", "Yo, working hard or hardly working?");
        Dialogue l19 = new Dialogue("Flamethrower", "I'd work harder if you gave me some motivation.");
        Dialogue l20 = new Dialogue("Player", "We're at war, that's not motivating?");
        Dialogue l21 = new Dialogue("Flamethrower", "I'm just saying, it gets a little boring out here. Lonely, even...");
        Dialogue l22 = new Dialogue("Player", "1. Don't you have allies to talk to?\n2. Again, we are still at war.\n3. Oh? Looks like you could use some company, doctor's orders.");

        DialogueScene ds4 = new DialogueScene(new Dialogue[]{l18, l19, l20, l21, l22}, 3);

        Dialogue l23 = new Dialogue("Flamethrower", "Is it getting hot in here or what? Maybe you can come back to my tower and check the heating?");
        Dialogue l24 = new Dialogue("Player", "Oh shit, I'll get an engineer right away.");
        Dialogue l25 = new Dialogue("Flamethrower", "... Got a sexy engineer outfit, do you?");
        Dialogue l26 = new Dialogue("Player", "1. How could an engineer outfit be sexy?\n2. Wouldn't you like to know? Be careful of flying sparks until I get there...\n3. No dude, I'm just management.");

        DialogueScene ds5 = new DialogueScene(new Dialogue[]{l23, l24, l25, l26}, 2);

        Dialogue l27 = new Dialogue("Flamethrower", "So, you doing anything after this war thing?");
        Dialogue l28 = new Dialogue("Player", "Probably just more war. Kind of my job.");
        Dialogue l29 = new Dialogue("Flamethrower", "Well, that sounds awfully repetitive. You need to spice things up.");
        Dialogue l30 = new Dialogue("Player", "What if I like my life to be predictable?");
        Dialogue l31 = new Dialogue("Flamethrower", "Well, after the war I'm gonna retire to the tropics. Got a hammock and everything. It's deluxe sized, comfortably fits two...");
        Dialogue l32 = new Dialogue("Player", "1. Well, I always did want to get a tan...\n2. Weird, you don't look THAT big.\n3. You are raising crazy death flags right now.");

        DialogueScene ds6 = new DialogueScene(new Dialogue[]{l27, l28, l29, l30, l31, l32}, 1);

        FlamethrowerDialogue = new TowerDialogue(new DialogueScene[]{ds4, ds5, ds6});


        Dialogue l33 = new Dialogue("Laser", "Hey, what's the rules on workplace romance?");
        Dialogue l34 = new Dialogue("Player", "I... don't think we've written those yet?");
        Dialogue l35 = new Dialogue("Laser", "Oh? So nothing about officers? Interesting.");
        Dialogue l36 = new Dialogue("Player", "1. Hey, if you've got any suggestions, my office is always open...\n2. In that case, we definitely need more rules.\n3. I suddenly feel like inventing Human Resources...");

        DialogueScene ds7 = new DialogueScene(new Dialogue[]{l33, l34, l35, l36}, 1);

        Dialogue l37 = new Dialogue("Laser", "Hey Commander, how's it looking?");
        Dialogue l38 = new Dialogue("Player", "What?");
        Dialogue l39 = new Dialogue("Laser", "You know, the war we're fighting. Are we actually winning?");
        Dialogue l40 = new Dialogue("Player", "1. Depends on your definition of 'winning'\n2. Well, not with that attitude!\n3. Well, having you by my side certainly helps handsome.");

        DialogueScene ds8 = new DialogueScene(new Dialogue[]{l37, l38, l39, l40}, 3);

        Dialogue l41 = new Dialogue("Player", "Hey, you doing alright? You look a bit jittery.");
        Dialogue l42 = new Dialogue("Laser", "Hm? Yea, I'll live. I just ran out of my favourite smokes is all");
        Dialogue l43 = new Dialogue("Player", "Oh? Smoking's your poison of choice?");
        Dialogue l44 = new Dialogue("Laser", "What can I say? Helps keep my hands steady. Besides, I need something to do up here");
        Dialogue l45 = new Dialogue("Laser", "How about you Commander, any poison of choice I should know about?");
        Dialogue l46 = new Dialogue("Player", "1. Only poison I keep on me is a Cyanide Pill.\n2. Well, looks like I only have 1 cigarette left myself. Looks like we'll have to share...\n3. Yea, I got a massive gambling addiction.");

        DialogueScene ds9 = new DialogueScene(new Dialogue[]{l41, l42, l43, l44, l45, l46}, 2);

        LaserDialogue = new TowerDialogue(new DialogueScene[]{ds7, ds8, ds9});


        Dialogue l47 = new Dialogue("Missile", "*Silence*");
        Dialogue l48 = new Dialogue("Player", "Hey Miss Isles, how's it going here?");
        Dialogue l49 = new Dialogue("Missile", "*Listens*");
        Dialogue l50 = new Dialogue("Player", "Man, battlefield's pretty crazy right now, huh?");
        Dialogue l51 = new Dialogue("Missile", "*Nods in agreement*");
        Dialogue l52 = new Dialogue("Player", "1. Ok then...\n2. You know, some enthuiasism wouldn't kill you.\n3. The strong, silent type, huh? I can work with that.");

        DialogueScene ds10 = new DialogueScene(new Dialogue[]{l47, l48, l49, l50, l51, l52}, 3);

        Dialogue l53 = new Dialogue("Player", "Hey, I'm not bothering you, right?");
        Dialogue l54 = new Dialogue("Missile", "*Listens stoicly*");
        Dialogue l55 = new Dialogue("Player", "I mean, you enjoy our talks, right?");
        Dialogue l56 = new Dialogue("Missile", "*Winks*");
        Dialogue l57 = new Dialogue("Player", "1. That makes me feel better. Good talk Miss Isles.\n2. Sorry, what was that?\n3. Still can't believe you just don't talk.");

        DialogueScene ds11 = new DialogueScene(new Dialogue[]{l53, l54, l55, l56, l57}, 1);

        Dialogue l58 = new Dialogue("Player", "Hey, what's your favourite colour?");
        Dialogue l59 = new Dialogue("Missile", "...");
        Dialogue l60 = new Dialogue("Player", "...");
        Dialogue l61 = new Dialogue("Missile", "Blue. What's yours?");
        Dialogue l62 = new Dialogue("Player", "1. HOLY CRAP, YOU TALK???\n2. Blue's also my favourite!\n 3. Booo. Red all the way.");

        DialogueScene ds12 = new DialogueScene(new Dialogue[]{l58, l59, l60, l61, l62}, 2);
        
        MissileDialogue = new TowerDialogue(new DialogueScene[]{ds10, ds11, ds12});
    }

    public void GunnerUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.AntiAir);
        Debug.Log("Gunner Upgraded to level 1!");
    }

    public void GunnerUpgrade2()
    {
        Upgrade2 = true;
        Range = 8;
        Firerate = 2;
        Debug.Log("Gunner Upgraded to level 2!");
    }

    public void GunnerUpgrade3()
    {
        Upgrade3 = true;
        Debug.Log("Gunner Upgraded to level 3!");
    }

    public void MissileUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ArmourPiercing);
        Debug.Log("Missile Upgraded to level 1!");
    }

    public void MissileUpgrade2()
    {
        Upgrade2 = true;
        Damage = 20;
        MissileCollisionManager temp = GetComponentInChildren<MissileCollisionManager>();
        temp.ExplosionRadius = 3;
        Debug.Log("Missile Upgraded to level 2!");
    }

    public void MissileUpgrade3()
    {
        Upgrade3 = true;
        MissileCollisionManager temp = GetComponentInChildren<MissileCollisionManager>();
        temp.ShellShock = true;
        //Debug.Log(temp.ShellShock);
        Debug.Log("Missile Upgraded to level 3!");
    }

    public void FlamethrowerUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ArmourPiercing);
        Debug.Log("Flamethrower Upgraded to level 1!");
    }

    public void FlamethrowerUpgrade2()
    {
        Upgrade2 = true;
        Firerate = 10;
        Debug.Log("Flamethrower Upgraded to level 2!");
    }

    public void FlamethrowerUpgrade3()
    {
        Upgrade3 = true;
        FlamethrowerDamage temp = GetComponent<FlamethrowerDamage>();
        temp.Cerbereus = true;

        temp.extraFireTriggers.SetActive(true);
        temp.FireEffect.startSpeed = 10;
        Debug.Log("Flamethrower Upgraded to level 3!");
    }

    public void LaserUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ColdHearted);
        Debug.Log("Laser Upgraded to level 1!");
    }

    public void LaserUpgrade2()
    {
        Upgrade2 = true;
        Firerate = 10;
        Debug.Log("Laser Upgraded to level 2!");
    }

    public void LaserUpgrade3()
    {
        Upgrade3 = true;
        LaserDamage temp = GetComponent<LaserDamage>();
        temp.BaseDamage = 1;
        Debug.Log("Laser Upgraded to level 3!");
    }
}
