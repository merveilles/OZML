using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class Bootstrap : MonoBehaviour {

    public string Url = "http://theinstructionlimit.com/collegiennes/test.ozml";
	public string Version;
	public bool AutoRefresh = false;
	public float RefreshTime = 10.0f;
	
	public GameObject[] ObjectFabs;
    public Material BaseMat;
	
	public Texture2D LoadingLogo;
	public Texture2D LoadingRing;
	
	public Ozml mainOzml;
    private delegate void NodeParser(XmlNode node);
    private Dictionary<string, NodeParser> ParseList;
    private Dictionary<string, GameObject> TypeList;
	
    private Dictionary<string, Material> MatList;
	private Dictionary<string, GameObject> ObjList;
	
	private bool Loading = true;
	private bool Booted = false;
	private string LoadedUrl;
    private float LogoAlpha = 1.0f;
    private Color BGColor;
    private Color FogColor;
	private float Rtimer = 9000.0f;

	// Use this for initialization
	void Awake () {
		if(Booted) return;
		
		Screen.lockCursor = true;
		
        //SetupType list
        TypeList = new Dictionary<string, GameObject>();
        for (int i = 0; i < ObjectFabs.Length; ++i ){
            TypeList.Add(ObjectFabs[i].name, ObjectFabs[i]);
        }

		//Setup calling table
		ParseList = new Dictionary<string, NodeParser>();
        ParseList.Add("ozml", ParseOzml);
        ParseList.Add("head", ParseHead);
        ParseList.Add("title", ParseTitle);
        ParseList.Add("scene", ParseScene);
        ParseList.Add("camera", ParseCamera);
        ParseList.Add("materials", ParseMaterials);
        ParseList.Add("geometry", ParseGeometry);
		
        ParseList.Add("cube", ParseObject);
        ParseList.Add("sphere", ParseObject);	
		
		Booted = true;
		if(!Application.isWebPlayer )
			StartOzml(Url); //Load url from app
		else
			Application.ExternalCall( "PassUrl", "") ; //Ask website to pass the url
	}
	
	void Update(){
		if (Input.GetKeyDown("escape"))
            Screen.lockCursor = false;
		
		if(Input.GetButtonDown("Fire1"))
			Screen.lockCursor = true;
		
		if(Input.GetKeyDown("r"))
			Refresh();
		
		Rtimer -= Time.deltaTime;
		if(Rtimer <= 0.0f){
			Refresh();
			Rtimer += RefreshTime;
		}
	}
	
	public void Refresh(){
		Debug.Log("Refreshing");
		StartCoroutine(FetchOzml(LoadedUrl, false));
	}
	
	public void FullRefresh(){
		Debug.Log("Full refreshing");
		StartCoroutine(FetchOzml(LoadedUrl, true));
	}
	
	public void StartOzml(string url){
		Debug.Log("LOAD REQUEST: "+ url);
		if(!Booted) Awake();
		StartCoroutine(FetchOzml(url, true));
	}
	
	void OnGUI(){
		
		if (Input.GetKey("i")){
			GUI.color = Color.red;
			GUI.Label(new Rect(20,0,100, 20), Version);
			GUI.Label(new Rect(20,20,400, 20), LoadedUrl);
		}
		
		if(Loading){
            Color newcolor = Color.white;
            newcolor.a = LogoAlpha;
            GUI.color = newcolor;
			int hheight = Screen.height >> 1;
			int hwidth = Screen.width >> 1;
			
			int hwlogo = LoadingLogo.width >> 1;
			int hhlogo = LoadingLogo.height >> 1;
			GUI.DrawTexture(new Rect(hwidth - hwlogo, hheight - hhlogo, LoadingLogo.width, LoadingLogo.height), LoadingLogo);
			
			int hwring = LoadingRing.width >> 1;
			int hhring = LoadingRing.height >> 1;

            Vector2 pivotPoint = new Vector2(hwidth, hheight);
			GUIUtility.RotateAroundPivot(Time.time * 10, pivotPoint);
			GUI.DrawTexture(new Rect(hwidth - hwring, hheight - hhring, LoadingRing.width, LoadingRing.height), LoadingRing);
		}
	}

    IEnumerator Process(bool Fullrefresh)
    {
        if (mainOzml.Head != null)
        {
            Debug.Log("Processing Head");
            BGColor = mainOzml.Head.Scene.Background;
			
			if(Fullrefresh){
				Camera.main.backgroundColor = Color.white;
			}
			else{
				Camera.main.backgroundColor = Color.white;
			}
			
			if(Fullrefresh){
				//Camera.main.fov = mainOzml.Head.Scene.Fov;
				//Camera.main.transform.position = mainOzml.Head.Camera.Position;
				//Camera.main.transform.rotation = Quaternion.Euler(mainOzml.Head.Camera.Rotation);
			}

            RenderSettings.fog = true;
			RenderSettings.fogDensity = mainOzml.Head.Scene.Fog.Density;
			
			FogColor = mainOzml.Head.Scene.Fog.Color;
			if(Fullrefresh)
				RenderSettings.fogColor = Color.white; //pure white
			else
				RenderSettings.fogColor = FogColor;
        }
        yield return 0;

        if (mainOzml.Materials != null)
        {
            Debug.Log("Processing " + mainOzml.Materials.Count + " Materials");
			if(MatList == null)
				MatList = new Dictionary<string, Material>();
			
            foreach (KeyValuePair<string, OzmlMaterial> pair in mainOzml.Materials)
            {
				Material mat;
				if( MatList.ContainsKey(pair.Key) )
					mat = MatList[pair.Key];
				else
					mat = new Material(BaseMat);
				
                Color newcol = pair.Value.AmbientDiffuse;
                newcol.a = pair.Value.Opacity;
                mat.name = pair.Key;
                mat.SetColor("_Color", newcol);
                mat.SetColor("_SpecColor", pair.Value.Specular);
                mat.SetColor("_Emission", pair.Value.Emissive);

                MatList[pair.Key] = mat;

                yield return 0;
            }
        }

        if (mainOzml.Objects != null)
        {
            Debug.Log("Processing " + mainOzml.Objects.Count + " Objects");
			if(ObjList == null)
				ObjList = new Dictionary<string, GameObject>();
			
            foreach (KeyValuePair<string, OzmlObject> pair in mainOzml.Objects)
            {
                if (TypeList.ContainsKey(pair.Value.Type))
                {
                    GameObject obj;
                    if (ObjList.ContainsKey(pair.Value.Id))
                        obj = ObjList[pair.Value.Id];
                    else
                        obj = Instantiate(TypeList[pair.Value.Type]) as GameObject;

                    obj.name = pair.Value.Id;
                    obj.transform.position = pair.Value.Position;
                    obj.transform.rotation = Quaternion.Euler(pair.Value.Rotation);
                    obj.transform.localScale = pair.Value.Size;
					obj.GetComponent<Interactivity>().bootstrap = this;
					obj.GetComponent<Interactivity>().LinkURL = pair.Value.Href;

                    if(MatList != null && MatList.ContainsKey(pair.Value.Mat))
                    {
                        obj.renderer.material = MatList[pair.Value.Mat];
                    }

                    ObjList[obj.name] = obj;
                    yield return 0;
                }
                else
                {
                    Debug.LogWarning("Unknown type: " + pair.Value.Type);
                }
            }
        }
	}

    IEnumerator FadeSettings()
    {
        float progress = 0;

        Color BGCColor = Color.white;
        Color FogCColor = RenderSettings.fogColor;

        //Fade logo
        while (progress < 1.0f) {
            progress += Time.deltaTime * 1.5f;
            LogoAlpha = 1.0f - progress;
            yield return 0;
        }

		LogoAlpha = 0.0f;
        Loading = false;
        progress = 0.0f;

        //Fade in fog settings and BG
        while (progress < 1.0f) {
            Camera.main.backgroundColor = Color.white;
            RenderSettings.fogColor = Color.white;

            progress += Time.deltaTime * 1.5f;
            yield return 0;
        }
		
		Camera.main.backgroundColor = BGColor;
		RenderSettings.fogColor = FogColor;
		
		progress = 0.0f;
		while (progress < 1.0f) {
            Camera.main.farClipPlane = Mathf.SmoothStep (Camera.main.nearClipPlane + 0.1f, 200.0f, progress);

            progress += Time.deltaTime * 0.15f;
            yield return 0;
        }
		
		Camera.main.farClipPlane = 200.0f;
    }

    IEnumerator FetchOzml(string url, bool Fullrefresh)
    {	
        //DOWNLOAD FILE
		Rtimer = 9000.0f;
        string ozmlText;
		LoadedUrl = url;
        Debug.Log("Fetching ozml file @ " + url);
        using (var www = new WWW(url))
        {
            while (!www.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
            ozmlText = www.text;
        }
        Debug.Log("Ozml text is : \n" + ozmlText);

        //Addition testing for encoding
        StringReader reader = new StringReader(ozmlText);
        if(ozmlText[0] != '<'){ //Skip BOM
            reader.Read();
        }

        //Create OzmlObject
        mainOzml = new Ozml();

        //PARSE FILE
        XmlDocument document = new XmlDocument();
        document.LoadXml(reader.ReadToEnd());
        ParseChilderen(document);


        //Process parsed file
        yield return StartCoroutine( Process(Fullrefresh) );

        //Fade fog and Loading screen
		if(Fullrefresh)
			StartCoroutine(FadeSettings());
		
		Rtimer = RefreshTime;
    }

    void ParseOzml(XmlNode node){
        ParseChilderen(node);
    }

    void ParseHead(XmlNode node)
    {
        mainOzml.Head = new OzmlHead();
        //No data in head itself
        ParseChilderen(node);
    }

    void ParseTitle(XmlNode node)
    {
        mainOzml.Head.Title = node.InnerText;
        //No childs
    }

    void ParseScene(XmlNode node)
    {
        mainOzml.Head.Scene = new OzmlScene();

        XmlAttributeCollection attributes = node.Attributes;
        string background = attributes.GetNamedItem("background").Value;
        string fog = attributes.GetNamedItem("fog").Value;
        string fov = attributes.GetNamedItem("fov").Value;

        Parsing.ParseRgb(background, ref mainOzml.Head.Scene.Background);
        Parsing.ParseFogParameters(fog, ref mainOzml.Head.Scene.Fog);
        Parsing.ParseDecimal(fov, ref mainOzml.Head.Scene.Fov);
        //No childs
    }

    void ParseCamera(XmlNode node)
    {
        mainOzml.Head.Camera = new OzmlCamera();

        XmlAttributeCollection attributes = node.Attributes;
        string position = attributes.GetNamedItem("position").Value;
        string rotation = attributes.GetNamedItem("rotation").Value;
		string speed = attributes.GetNamedItem("speed").Value;

        Parsing.ParseVector3(position, ref mainOzml.Head.Camera.Position);
        Parsing.ParseVector3(rotation, ref mainOzml.Head.Camera.Rotation);
		Parsing.ParseDecimal(speed, ref mainOzml.Head.Camera.Speed);
        //No childs
    }

    void ParseMaterials(XmlNode node)
    {
        mainOzml.Materials = new Dictionary<string, OzmlMaterial>();
        IEnumerable<OzmlMaterial> result = Parsing.ParseMaterials(node.InnerText);
        foreach(OzmlMaterial mat in result){
            mainOzml.Materials[mat.Name] = mat;
        }
    }

    void ParseObject(XmlNode node)
    {
        OzmlObject obj = new OzmlObject();

        XmlAttributeCollection attributes = node.Attributes;
        string id = attributes.GetNamedItem("name").Value;
		
        string position; 
		try
		{ position = attributes.GetNamedItem("position").Value; }
		catch
		{ position = "0,0,0"; }
		
        string rotation;
		try
		{ rotation = attributes.GetNamedItem("rotation").Value; }
		catch
		{ rotation = "0,0,0"; }
		
        string size = attributes.GetNamedItem("size").Value;
        string material = attributes.GetNamedItem("material").Value;
        string type = node.Name;
        string href;
		try
		{ href = attributes.GetNamedItem("href").Value; }
		catch
		{ href = ""; }

        obj.Id = id;
        Parsing.ParseVector3(position, ref obj.Position);
        Parsing.ParseVector3(rotation, ref obj.Rotation);
        Parsing.ParseVector3(size, ref obj.Size);
        obj.Mat = material;
        obj.Type = type;
		obj.Href = href;

		mainOzml.Objects[obj.Id] = obj;
        ParseChilderen(node);
    }

    void ParseGeometry(XmlNode node)
    {
        mainOzml.Objects = new Dictionary<string, OzmlObject>();
        //No data in geometry itself
        ParseChilderen(node);
    }

    void ParseChilderen (XmlNode node){
        XmlNodeList childs = node.ChildNodes;
        foreach (XmlNode child in childs)
        {
            string name = child.Name;
            if (ParseList.ContainsKey(name))
            {
                ParseList[name](child);
            }
            else
            {
                Debug.Log("Unknown <" + name + ">");
            }
        }
    }
}