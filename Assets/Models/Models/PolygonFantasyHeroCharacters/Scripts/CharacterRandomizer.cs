// character randomizer version 1.30
using System.Collections.Generic;
using UnityEngine;

namespace PsychoticLab
{
    public enum Gender { Male, Female }
    public enum Race { Human, Elf }
    public enum SkinColor { White, Brown, Black, Elf }
    public enum Elements { Yes, No }
    public enum HeadCovering { HeadCoverings_Base_Hair, HeadCoverings_No_FacialHair, HeadCoverings_No_Hair }
    public enum FacialHair { Yes, No }

    public class CharacterRandomizer : MonoBehaviour
    {
        [Header("Demo Settings")]
        public bool repeatOnPlay = false;
        public float shuffleSpeed = 0.7f;

        [Header("Material")]
        public Material mat;

        [Header("Gear Colors")]
        public Color[] primary = { new Color(0.2862745f, 0.4f, 0.4941177f), new Color(0.4392157f, 0.1960784f, 0.172549f), new Color(0.3529412f, 0.3803922f, 0.2705882f), new Color(0.682353f, 0.4392157f, 0.2196079f), new Color(0.4313726f, 0.2313726f, 0.2705882f), new Color(0.5921569f, 0.4941177f, 0.2588235f), new Color(0.482353f, 0.4156863f, 0.3529412f), new Color(0.2352941f, 0.2352941f, 0.2352941f), new Color(0.2313726f, 0.4313726f, 0.4156863f) };
        public Color[] secondary = { new Color(0.7019608f, 0.6235294f, 0.4666667f), new Color(0.7372549f, 0.7372549f, 0.7372549f), new Color(0.1647059f, 0.1647059f, 0.1647059f), new Color(0.2392157f, 0.2509804f, 0.1882353f) };

        [Header("Metal Colors")]
        public Color[] metalPrimary = { new Color(0.6705883f, 0.6705883f, 0.6705883f), new Color(0.5568628f, 0.5960785f, 0.6392157f), new Color(0.5568628f, 0.6235294f, 0.6f), new Color(0.6313726f, 0.6196079f, 0.5568628f), new Color(0.6980392f, 0.6509804f, 0.6196079f) };
        public Color[] metalSecondary = { new Color(0.3921569f, 0.4039216f, 0.4117647f), new Color(0.4784314f, 0.5176471f, 0.5450981f), new Color(0.3764706f, 0.3607843f, 0.3372549f), new Color(0.3254902f, 0.3764706f, 0.3372549f), new Color(0.4f, 0.4039216f, 0.3568628f) };

        [Header("Leather Colors")]
        public Color[] leatherPrimary;
        public Color[] leatherSecondary;

        [Header("Skin Colors")]
        public Color[] whiteSkin = { new Color(1f, 0.8000001f, 0.682353f) };
        public Color[] brownSkin = { new Color(0.8196079f, 0.6352941f, 0.4588236f) };
        public Color[] blackSkin = { new Color(0.5647059f, 0.4078432f, 0.3137255f) };
        public Color[] elfSkin = { new Color(0.9607844f, 0.7843138f, 0.7294118f) };

        [Header("Hair Colors")]
        public Color[] whiteHair = { new Color(0.3098039f, 0.254902f, 0.1764706f), new Color(0.2196079f, 0.2196079f, 0.2196079f), new Color(0.8313726f, 0.6235294f, 0.3607843f), new Color(0.8901961f, 0.7803922f, 0.5490196f), new Color(0.8000001f, 0.8196079f, 0.8078432f), new Color(0.6862745f, 0.4f, 0.2352941f), new Color(0.5450981f, 0.427451f, 0.2156863f), new Color(0.8470589f, 0.4666667f, 0.2470588f) };
        public Color whiteStubble = new Color(0.8039216f, 0.7019608f, 0.6313726f);
        public Color[] brownHair = { new Color(0.3098039f, 0.254902f, 0.1764706f), new Color(0.1764706f, 0.1686275f, 0.1686275f), new Color(0.3843138f, 0.2352941f, 0.0509804f), new Color(0.6196079f, 0.6196079f, 0.6196079f), new Color(0.6196079f, 0.6196079f, 0.6196079f) };
        public Color brownStubble = new Color(0.6588235f, 0.572549f, 0.4627451f);
        public Color[] blackHair = { new Color(0.2431373f, 0.2039216f, 0.145098f), new Color(0.1764706f, 0.1686275f, 0.1686275f), new Color(0.1764706f, 0.1686275f, 0.1686275f) };
        public Color blackStubble = new Color(0.3882353f, 0.2901961f, 0.2470588f);
        public Color[] elfHair = { new Color(0.9764706f, 0.9686275f, 0.9568628f), new Color(0.1764706f, 0.1686275f, 0.1686275f), new Color(0.8980393f, 0.7764707f, 0.6196079f) };
        public Color elfStubble = new Color(0.8627452f, 0.7294118f, 0.6862745f);

        [Header("Scar Colors")]
        public Color whiteScar = new Color(0.9294118f, 0.6862745f, 0.5921569f);
        public Color brownScar = new Color(0.6980392f, 0.5450981f, 0.4f);
        public Color blackScar = new Color(0.4235294f, 0.3176471f, 0.282353f);
        public Color elfScar = new Color(0.8745099f, 0.6588235f, 0.6313726f);

        [Header("Body Art Colors")]
        public Color[] bodyArt = { new Color(0.0509804f, 0.6745098f, 0.9843138f), new Color(0.7215686f, 0.2666667f, 0.2666667f), new Color(0.3058824f, 0.7215686f, 0.6862745f), new Color(0.9254903f, 0.882353f, 0.8509805f), new Color(0.3098039f, 0.7058824f, 0.3137255f), new Color(0.5294118f, 0.3098039f, 0.6470588f), new Color(0.8666667f, 0.7764707f, 0.254902f), new Color(0.2392157f, 0.4588236f, 0.8156863f) };

        // list of enabed objects on character
        [HideInInspector]
        public List<GameObject> enabledObjects = new List<GameObject>();

        // character object lists
        // male list
        [HideInInspector]
        public CharacterObjectGroups male;

        // female list
        [HideInInspector]
        public CharacterObjectGroups female;

        // universal list
        [HideInInspector]
        public CharacterObjectListsAllGender allGender;

        // reference to camera transform, used for rotation around the model during or after a randomization (this is sourced from Camera.main, so the main camera must be in the scene for this to work)
        private Transform camHolder;

        // cam rotation x
        private float x = 16;

        // cam rotation y
        private float y = -30;

        // randomize character creating button
        private void OnGUI()
        {
            /*
            if (GUI.Button(new Rect(10, 10, 150, 50), "Randomize Character"))
            {
                // call randomization method
                Randomize();
            }
            */

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 24;
            GUI.Label(new Rect(10, 10, 150, 50), "Hold Right Mouse Button Down\nor use W A S D To Rotate.", style);
        }

        private void Start()
        {
            // rebuild all lists
            this.BuildLists();

            // disable any enabled objects before clear
            if (this.enabledObjects.Count != 0)
            {
                foreach (GameObject g in this.enabledObjects)
                {
                    g.SetActive(false);
                }
            }

            // clear enabled objects list
            this.enabledObjects.Clear();

            // set default male character
            this.ActivateItem(this.male.headAllElements[0]);
            this.ActivateItem(this.male.eyebrow[0]);
            this.ActivateItem(this.male.facialHair[0]);
            this.ActivateItem(this.male.torso[0]);
            this.ActivateItem(this.male.arm_Upper_Right[0]);
            this.ActivateItem(this.male.arm_Upper_Left[0]);
            this.ActivateItem(this.male.arm_Lower_Right[0]);
            this.ActivateItem(this.male.arm_Lower_Left[0]);
            this.ActivateItem(this.male.hand_Right[0]);
            this.ActivateItem(this.male.hand_Left[0]);
            this.ActivateItem(this.male.hips[0]);
            this.ActivateItem(this.male.leg_Right[0]);
            this.ActivateItem(this.male.leg_Left[0]);

            // setting up the camera position, rotation, and reference for use
            Transform cam = Camera.main.transform;
            if (cam)
            {
                cam.position = this.transform.position + new Vector3(0, 0.3f, 2);
                cam.rotation = Quaternion.Euler(0, -180, 0);
                this.camHolder = new GameObject().transform;
                this.camHolder.position = this.transform.position + new Vector3(0, 1, 0);
                cam.LookAt(this.camHolder);
                cam.SetParent(this.camHolder);
            }

            // if repeat on play is checked in the inspector, repeat the randomize method based on the shuffle speed, also defined in the inspector
            if (this.repeatOnPlay)
            {
                this.InvokeRepeating("Randomize", this.shuffleSpeed, this.shuffleSpeed);
            }
        }

        private void Update()
        {
            if (this.camHolder)
            {
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    this.x += 1 * Input.GetAxis("Mouse X");
                    this.y -= 1 * Input.GetAxis("Mouse Y");
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    this.x -= 1 * Input.GetAxis("Horizontal");
                    this.y -= 1 * Input.GetAxis("Vertical");
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void LateUpdate()
        {
            // method for handling the camera rotation around the character
            if (this.camHolder)
            {
                this.y = Mathf.Clamp(this.y, -45, 15);
                this.camHolder.eulerAngles = new Vector3(this.y, this.x, 0.0f);
            }
        }

        // character randomization method
        private void Randomize()
        {
            // initialize settings
            Gender gender = Gender.Male;
            Race race = Race.Human;
            SkinColor skinColor = SkinColor.White;
            Elements elements = Elements.Yes;
            HeadCovering headCovering = HeadCovering.HeadCoverings_Base_Hair;
            FacialHair facialHair = FacialHair.Yes;

            // disable any enabled objects before clear
            if (this.enabledObjects.Count != 0)
            {
                foreach (GameObject g in this.enabledObjects)
                {
                    g.SetActive(false);
                }
            }

            // clear enabled objects list (all objects now disabled)
            this.enabledObjects.Clear();

            // roll for gender
            if (!this.GetPercent(50))
            {
                gender = Gender.Female;
            }

            // roll for human (70% chance, 30% chance for elf)
            if (!this.GetPercent(70))
            {
                race = Race.Elf;
            }

            // roll for facial elements (beard, eyebrows)
            if (!this.GetPercent(50))
            {
                elements = Elements.No;
            }

            // select head covering 33% chance for each
            int headCoveringRoll = Random.Range(0, 100);
            // HeadCoverings_Base_Hair
            if (headCoveringRoll <= 33)
            {
                headCovering = HeadCovering.HeadCoverings_Base_Hair;
            }
            // HeadCoverings_No_FacialHair
            if (headCoveringRoll > 33 && headCoveringRoll < 66)
            {
                headCovering = HeadCovering.HeadCoverings_No_FacialHair;
            }
            // HeadCoverings_No_Hair
            if (headCoveringRoll >= 66)
            {
                headCovering = HeadCovering.HeadCoverings_No_Hair;
            }

            // select skin color if human, otherwise set skin color to elf
            switch (race)
            {
                case Race.Human:
                    // select human skin 33% chance for each
                    int colorRoll = Random.Range(0, 100);
                    // select white skin
                    if (colorRoll <= 33)
                    {
                        skinColor = SkinColor.White;
                    }
                    // select brown skin
                    if (colorRoll > 33 && colorRoll < 66)
                    {
                        skinColor = SkinColor.Brown;
                    }
                    // select black skin
                    if (colorRoll >= 66)
                    {
                        skinColor = SkinColor.Black;
                    }

                    break;
                case Race.Elf:
                    // select elf skin
                    skinColor = SkinColor.Elf;
                    break;
            }

            //roll for gender
            switch (gender)
            {
                case Gender.Male:
                    // roll for facial hair if male
                    if (!this.GetPercent(50))
                    {
                        facialHair = FacialHair.No;
                    }

                    // initialize randomization
                    this.RandomizeByVariable(this.male, gender, elements, race, facialHair, skinColor, headCovering);
                    break;

                case Gender.Female:

                    // no facial hair if female
                    facialHair = FacialHair.No;

                    // initialize randomization
                    this.RandomizeByVariable(this.female, gender, elements, race, facialHair, skinColor, headCovering);
                    break;
            }
        }

        // randomization method based on previously selected variables
        private void RandomizeByVariable(CharacterObjectGroups cog, Gender gender, Elements elements, Race race, FacialHair facialHair, SkinColor skinColor, HeadCovering headCovering)
        {
            // if facial elements are enabled
            switch (elements)
            {
                case Elements.Yes:
                    //select head with all elements
                    if (cog.headAllElements.Count != 0)
                    {
                        this.ActivateItem(cog.headAllElements[Random.Range(0, cog.headAllElements.Count)]);
                    }

                    //select eyebrows
                    if (cog.eyebrow.Count != 0)
                    {
                        this.ActivateItem(cog.eyebrow[Random.Range(0, cog.eyebrow.Count)]);
                    }

                    //select facial hair (conditional)
                    if (cog.facialHair.Count != 0 && facialHair == FacialHair.Yes && gender == Gender.Male && headCovering != HeadCovering.HeadCoverings_No_FacialHair)
                    {
                        this.ActivateItem(cog.facialHair[Random.Range(0, cog.facialHair.Count)]);
                    }

                    // select hair attachment
                    switch (headCovering)
                    {
                        case HeadCovering.HeadCoverings_Base_Hair:
                            // set hair attachment to index 1
                            if (this.allGender.all_Hair.Count != 0)
                            {
                                this.ActivateItem(this.allGender.all_Hair[1]);
                            }

                            if (this.allGender.headCoverings_Base_Hair.Count != 0)
                            {
                                this.ActivateItem(this.allGender.headCoverings_Base_Hair[Random.Range(0, this.allGender.headCoverings_Base_Hair.Count)]);
                            }

                            break;
                        case HeadCovering.HeadCoverings_No_FacialHair:
                            // no facial hair attachment
                            if (this.allGender.all_Hair.Count != 0)
                            {
                                this.ActivateItem(this.allGender.all_Hair[Random.Range(0, this.allGender.all_Hair.Count)]);
                            }

                            if (this.allGender.headCoverings_No_FacialHair.Count != 0)
                            {
                                this.ActivateItem(this.allGender.headCoverings_No_FacialHair[Random.Range(0, this.allGender.headCoverings_No_FacialHair.Count)]);
                            }

                            break;
                        case HeadCovering.HeadCoverings_No_Hair:
                            // select hair attachment
                            if (this.allGender.headCoverings_No_Hair.Count != 0)
                            {
                                this.ActivateItem(this.allGender.all_Hair[Random.Range(0, this.allGender.all_Hair.Count)]);
                            }
                            // if not human
                            if (race != Race.Human)
                            {
                                // select elf ear attachment
                                if (this.allGender.elf_Ear.Count != 0)
                                {
                                    this.ActivateItem(this.allGender.elf_Ear[Random.Range(0, this.allGender.elf_Ear.Count)]);
                                }
                            }
                            break;
                    }
                    break;

                case Elements.No:
                    //select head with no elements
                    if (cog.headNoElements.Count != 0)
                    {
                        this.ActivateItem(cog.headNoElements[Random.Range(0, cog.headNoElements.Count)]);
                    }

                    break;
            }

            // select torso starting at index 1
            if (cog.torso.Count != 0)
            {
                this.ActivateItem(cog.torso[Random.Range(1, cog.torso.Count)]);
            }

            // determine chance for upper arms to be different and activate
            if (cog.arm_Upper_Right.Count != 0)
            {
                this.RandomizeLeftRight(cog.arm_Upper_Right, cog.arm_Upper_Left, 15);
            }

            // determine chance for lower arms to be different and activate
            if (cog.arm_Lower_Right.Count != 0)
            {
                this.RandomizeLeftRight(cog.arm_Lower_Right, cog.arm_Lower_Left, 15);
            }

            // determine chance for hands to be different and activate
            if (cog.hand_Right.Count != 0)
            {
                this.RandomizeLeftRight(cog.hand_Right, cog.hand_Left, 15);
            }

            // select hips starting at index 1
            if (cog.hips.Count != 0)
            {
                this.ActivateItem(cog.hips[Random.Range(1, cog.hips.Count)]);
            }

            // determine chance for legs to be different and activate
            if (cog.leg_Right.Count != 0)
            {
                this.RandomizeLeftRight(cog.leg_Right, cog.leg_Left, 15);
            }

            // select chest attachment
            if (this.allGender.chest_Attachment.Count != 0)
            {
                this.ActivateItem(this.allGender.chest_Attachment[Random.Range(0, this.allGender.chest_Attachment.Count)]);
            }

            // select back attachment
            if (this.allGender.back_Attachment.Count != 0)
            {
                this.ActivateItem(this.allGender.back_Attachment[Random.Range(0, this.allGender.back_Attachment.Count)]);
            }

            // determine chance for shoulder attachments to be different and activate
            if (this.allGender.shoulder_Attachment_Right.Count != 0)
            {
                this.RandomizeLeftRight(this.allGender.shoulder_Attachment_Right, this.allGender.shoulder_Attachment_Left, 10);
            }

            // determine chance for elbow attachments to be different and activate
            if (this.allGender.elbow_Attachment_Right.Count != 0)
            {
                this.RandomizeLeftRight(this.allGender.elbow_Attachment_Right, this.allGender.elbow_Attachment_Left, 10);
            }

            // select hip attachment
            if (this.allGender.hips_Attachment.Count != 0)
            {
                this.ActivateItem(this.allGender.hips_Attachment[Random.Range(0, this.allGender.hips_Attachment.Count)]);
            }

            // determine chance for knee attachments to be different and activate
            if (this.allGender.knee_Attachement_Right.Count != 0)
            {
                this.RandomizeLeftRight(this.allGender.knee_Attachement_Right, this.allGender.knee_Attachement_Left, 10);
            }

            // start randomization of the random characters colors
            this.RandomizeColors(skinColor);
        }

        // handle randomization of the random characters colors
        private void RandomizeColors(SkinColor skinColor)
        {
            // set skin and hair colors based on skin color roll
            switch (skinColor)
            {
                case SkinColor.White:
                    // randomize and set white skin, hair, stubble, and scar color
                    this.RandomizeAndSetHairSkinColors("White", this.whiteSkin, this.whiteHair, this.whiteStubble, this.whiteScar);
                    break;

                case SkinColor.Brown:
                    // randomize and set brown skin, hair, stubble, and scar color
                    this.RandomizeAndSetHairSkinColors("Brown", this.brownSkin, this.brownHair, this.brownStubble, this.brownScar);
                    break;

                case SkinColor.Black:
                    // randomize and black elf skin, hair, stubble, and scar color
                    this.RandomizeAndSetHairSkinColors("Black", this.blackSkin, this.blackHair, this.blackStubble, this.blackScar);
                    break;

                case SkinColor.Elf:
                    // randomize and set elf skin, hair, stubble, and scar color
                    this.RandomizeAndSetHairSkinColors("Elf", this.elfSkin, this.elfHair, this.elfStubble, this.elfScar);
                    break;
            }

            // randomize and set primary color
            if (this.primary.Length != 0)
            {
                this.mat.SetColor("_Color_Primary", this.primary[Random.Range(0, this.primary.Length)]);
            }
            else
            {
                Debug.Log("No Primary Colors Specified In The Inspector");
            }

            // randomize and set secondary color
            if (this.secondary.Length != 0)
            {
                this.mat.SetColor("_Color_Secondary", this.secondary[Random.Range(0, this.secondary.Length)]);
            }
            else
            {
                Debug.Log("No Secondary Colors Specified In The Inspector");
            }

            // randomize and set primary metal color
            if (this.metalPrimary.Length != 0)
            {
                this.mat.SetColor("_Color_Metal_Primary", this.metalPrimary[Random.Range(0, this.metalPrimary.Length)]);
            }
            else
            {
                Debug.Log("No Primary Metal Colors Specified In The Inspector");
            }

            // randomize and set secondary metal color
            if (this.metalSecondary.Length != 0)
            {
                this.mat.SetColor("_Color_Metal_Secondary", this.metalSecondary[Random.Range(0, this.metalSecondary.Length)]);
            }
            else
            {
                Debug.Log("No Secondary Metal Colors Specified In The Inspector");
            }

            // randomize and set primary leather color
            if (this.leatherPrimary.Length != 0)
            {
                this.mat.SetColor("_Color_Leather_Primary", this.leatherPrimary[Random.Range(0, this.leatherPrimary.Length)]);
            }
            else
            {
                Debug.Log("No Primary Leather Colors Specified In The Inspector");
            }

            // randomize and set secondary leather color
            if (this.leatherSecondary.Length != 0)
            {
                this.mat.SetColor("_Color_Leather_Secondary", this.leatherSecondary[Random.Range(0, this.leatherSecondary.Length)]);
            }
            else
            {
                Debug.Log("No Secondary Leather Colors Specified In The Inspector");
            }

            // randomize and set body art color
            if (this.bodyArt.Length != 0)
            {
                this.mat.SetColor("_Color_BodyArt", this.bodyArt[Random.Range(0, this.bodyArt.Length)]);
            }
            else
            {
                Debug.Log("No Body Art Colors Specified In The Inspector");
            }

            // randomize and set body art amount
            this.mat.SetFloat("_BodyArt_Amount", Random.Range(0.0f, 1.0f));
        }

        private void RandomizeAndSetHairSkinColors(string info, Color[] skin, Color[] hair, Color stubble, Color scar)
        {
            // randomize and set elf skin color
            if (skin.Length != 0)
            {
                this.mat.SetColor("_Color_Skin", skin[Random.Range(0, skin.Length)]);
            }
            else
            {
                Debug.Log("No " + info + " Skin Colors Specified In The Inspector");
            }

            // randomize and set elf hair color
            if (hair.Length != 0)
            {
                this.mat.SetColor("_Color_Hair", hair[Random.Range(0, hair.Length)]);
            }
            else
            {
                Debug.Log("No " + info + " Hair Colors Specified In The Inspector");
            }

            // set stubble color
            this.mat.SetColor("_Color_Stubble", stubble);

            // set scar color
            this.mat.SetColor("_Color_Scar", scar);
        }

        // method for handling the chance of left/right items to be differnt (such as shoulders, hands, legs, arms)
        private void RandomizeLeftRight(List<GameObject> objectListRight, List<GameObject> objectListLeft, int rndPercent)
        {
            // rndPercent = chance for left item to be different

            // stored right index
            int index = Random.Range(0, objectListRight.Count);

            // enable item from list using index
            this.ActivateItem(objectListRight[index]);

            // roll for left item mismatch, if true randomize index based on left item list
            if (this.GetPercent(rndPercent))
            {
                index = Random.Range(0, objectListLeft.Count);
            }

            // enable left item from list using index
            this.ActivateItem(objectListLeft[index]);
        }

        // enable game object and add it to the enabled objects list
        private void ActivateItem(GameObject go)
        {
            // enable item
            go.SetActive(true);

            // add item to the enabled items list
            this.enabledObjects.Add(go);
        }

        private Color ConvertColor(int r, int g, int b)
        {
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1);
        }

        // method for rolling percentages (returns true/false)
        private bool GetPercent(int pct)
        {
            bool p = false;
            int roll = Random.Range(0, 100);
            if (roll <= pct)
            {
                p = true;
            }
            return p;
        }

        // build all item lists for use in randomization
        private void BuildLists()
        {
            //build out male lists
            this.BuildList(this.male.headAllElements, "Male_Head_All_Elements");
            this.BuildList(this.male.headNoElements, "Male_Head_No_Elements");
            this.BuildList(this.male.eyebrow, "Male_01_Eyebrows");
            this.BuildList(this.male.facialHair, "Male_02_FacialHair");
            this.BuildList(this.male.torso, "Male_03_Torso");
            this.BuildList(this.male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
            this.BuildList(this.male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
            this.BuildList(this.male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
            this.BuildList(this.male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
            this.BuildList(this.male.hand_Right, "Male_08_Hand_Right");
            this.BuildList(this.male.hand_Left, "Male_09_Hand_Left");
            this.BuildList(this.male.hips, "Male_10_Hips");
            this.BuildList(this.male.leg_Right, "Male_11_Leg_Right");
            this.BuildList(this.male.leg_Left, "Male_12_Leg_Left");

            //build out female lists
            this.BuildList(this.female.headAllElements, "Female_Head_All_Elements");
            this.BuildList(this.female.headNoElements, "Female_Head_No_Elements");
            this.BuildList(this.female.eyebrow, "Female_01_Eyebrows");
            this.BuildList(this.female.facialHair, "Female_02_FacialHair");
            this.BuildList(this.female.torso, "Female_03_Torso");
            this.BuildList(this.female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
            this.BuildList(this.female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
            this.BuildList(this.female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
            this.BuildList(this.female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
            this.BuildList(this.female.hand_Right, "Female_08_Hand_Right");
            this.BuildList(this.female.hand_Left, "Female_09_Hand_Left");
            this.BuildList(this.female.hips, "Female_10_Hips");
            this.BuildList(this.female.leg_Right, "Female_11_Leg_Right");
            this.BuildList(this.female.leg_Left, "Female_12_Leg_Left");

            // build out all gender lists
            this.BuildList(this.allGender.all_Hair, "All_01_Hair");
            this.BuildList(this.allGender.all_Head_Attachment, "All_02_Head_Attachment");
            this.BuildList(this.allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
            this.BuildList(this.allGender.headCoverings_No_FacialHair, "HeadCoverings_No_FacialHair");
            this.BuildList(this.allGender.headCoverings_No_Hair, "HeadCoverings_No_Hair");
            this.BuildList(this.allGender.chest_Attachment, "All_03_Chest_Attachment");
            this.BuildList(this.allGender.back_Attachment, "All_04_Back_Attachment");
            this.BuildList(this.allGender.shoulder_Attachment_Right, "All_05_Shoulder_Attachment_Right");
            this.BuildList(this.allGender.shoulder_Attachment_Left, "All_06_Shoulder_Attachment_Left");
            this.BuildList(this.allGender.elbow_Attachment_Right, "All_07_Elbow_Attachment_Right");
            this.BuildList(this.allGender.elbow_Attachment_Left, "All_08_Elbow_Attachment_Left");
            this.BuildList(this.allGender.hips_Attachment, "All_09_Hips_Attachment");
            this.BuildList(this.allGender.knee_Attachement_Right, "All_10_Knee_Attachement_Right");
            this.BuildList(this.allGender.knee_Attachement_Left, "All_11_Knee_Attachement_Left");
            this.BuildList(this.allGender.elf_Ear, "Elf_Ear");
        }

        // called from the BuildLists method
        private void BuildList(List<GameObject> targetList, string characterPart)
        {
            Transform[] rootTransform = this.gameObject.GetComponentsInChildren<Transform>();

            // declare target root transform
            Transform targetRoot = null;

            // find character parts parent object in the scene
            foreach (Transform t in rootTransform)
            {
                if (t.gameObject.name == characterPart)
                {
                    targetRoot = t;
                    break;
                }
            }

            // clears targeted list of all objects
            targetList.Clear();

            // cycle through all child objects of the parent object
            for (int i = 0; i < targetRoot.childCount; i++)
            {
                // get child gameobject index i
                GameObject go = targetRoot.GetChild(i).gameObject;

                // disable child object
                go.SetActive(false);

                // add object to the targeted object list
                targetList.Add(go);

                // collect the material for the random character, only if null in the inspector;
                if (!this.mat)
                {
                    if (go.GetComponent<SkinnedMeshRenderer>())
                    {
                        this.mat = go.GetComponent<SkinnedMeshRenderer>().material;
                    }
                }
            }
        }
    }

    // classe for keeping the lists organized, allows for simple switching from male/female objects
    [System.Serializable]
    public class CharacterObjectGroups
    {
        public List<GameObject> headAllElements;
        public List<GameObject> headNoElements;
        public List<GameObject> eyebrow;
        public List<GameObject> facialHair;
        public List<GameObject> torso;
        public List<GameObject> arm_Upper_Right;
        public List<GameObject> arm_Upper_Left;
        public List<GameObject> arm_Lower_Right;
        public List<GameObject> arm_Lower_Left;
        public List<GameObject> hand_Right;
        public List<GameObject> hand_Left;
        public List<GameObject> hips;
        public List<GameObject> leg_Right;
        public List<GameObject> leg_Left;
    }

    // classe for keeping the lists organized, allows for organization of the all gender items
    [System.Serializable]
    public class CharacterObjectListsAllGender
    {
        public List<GameObject> headCoverings_Base_Hair;
        public List<GameObject> headCoverings_No_FacialHair;
        public List<GameObject> headCoverings_No_Hair;
        public List<GameObject> all_Hair;
        public List<GameObject> all_Head_Attachment;
        public List<GameObject> chest_Attachment;
        public List<GameObject> back_Attachment;
        public List<GameObject> shoulder_Attachment_Right;
        public List<GameObject> shoulder_Attachment_Left;
        public List<GameObject> elbow_Attachment_Right;
        public List<GameObject> elbow_Attachment_Left;
        public List<GameObject> hips_Attachment;
        public List<GameObject> knee_Attachement_Right;
        public List<GameObject> knee_Attachement_Left;
        public List<GameObject> all_12_Extra;
        public List<GameObject> elf_Ear;
    }
}