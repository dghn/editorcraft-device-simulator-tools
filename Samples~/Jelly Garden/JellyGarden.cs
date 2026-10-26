using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EditorCraft.Samples
{
    // Self-contained uGUI sample. No art downloads, game SDKs or project settings required.
    public sealed class JellyGarden : MonoBehaviour
    {
        const int Columns = 7, Rows = 7;
        readonly Color[] colors = { Hex("FF738E"), Hex("FFC94E"), Hex("7EDAC0"), Hex("A79BFF"), Hex("68CFFF") };
        readonly int[,] cells = new int[Columns, Rows];
        readonly RectTransform[,] tiles = new RectTransform[Columns, Rows];
        readonly List<Object> owned = new List<Object>();
        System.Random random = new System.Random(42);
        RectTransform safe, stage, board, progress;
        Text movesLabel, scoreLabel, hint;
        Font font;
        Sprite round, circle;
        int moves = 24, score;
        readonly int[] targets = {18,12,15};
        readonly Text[] targetLabels = new Text[3];
        bool Won => targets[0] == 0 && targets[1] == 0 && targets[2] == 0;
        bool busy;
        Vector2 lastSize;
        Rect lastSafe;
        public int Score => score;
        public int Moves => moves;
        public bool Busy => busy;

        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        void Start()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            round = Shape(20); circle = Shape(64);
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<GraphicRaycaster>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(900, 1600); scaler.matchWidthOrHeight = .5f;
            var root = (RectTransform)transform;
            var bg = Panel("Sky", root, Hex("DDF4EF")); Stretch(bg);
            // Oversized scenery intentionally extends outside the safe area.
            Blob(bg, new Vector2(-310,-550), new Vector2(1100,750), "BCE6CC");
            Blob(bg, new Vector2(440,-660), new Vector2(1200,900), "9DDAC5");
            Blob(bg, new Vector2(80,-820), new Vector2(1500,800), "78C5AD");
            Blob(bg, new Vector2(-300,660), new Vector2(380,140), "F4FFFA");
            Blob(bg, new Vector2(300,520), new Vector2(300,110), "F4FFFA");
            safe = Rect("Safe area", root);
            stage = Rect("Game", safe); stage.sizeDelta = new Vector2(760, 1300);
            var title = Label("JELLY", stage, new Vector2(0,540), new Vector2(650,130), 100, Hex("52619B"));
            title.fontStyle = FontStyle.Bold;
            Label("G A R D E N", stage, new Vector2(0,466), new Vector2(600,58), 32, Hex("658F88"));
            Pill(stage, "LEVEL 12", new Vector2(0,383), new Vector2(260,66), "8797D6", 29);
            var goals = Panel("Goals", stage, Hex("FFF9E9")); Place(goals, 0,270,700,142);
            Shadow(goals.gameObject, Hex("B5CEBB"), new Vector2(0,-8));
            Label("COLLECT JELLIES", goals, new Vector2(-116,38), new Vector2(390,38), 23, Hex("89958C"));
            for (int i=0; i<3; i++) {
                var jelly = Jelly(goals, i, new Vector2(-260+i*133,-22), 58);
                targetLabels[i]=Label(targets[i].ToString(), goals, new Vector2(-213+i*133,-22), new Vector2(60,44), 27, Hex("526078"));
            }
            var movesBox = Panel("Moves", goals, Hex("F8E8BD")); Place(movesBox,250,0,148,116);
            Label("MOVES",movesBox,new Vector2(0,31),new Vector2(130,30),21,Hex("A78D61"));
            movesLabel=Label("24",movesBox,new Vector2(0,-15),new Vector2(130,63),48,Hex("71614A"));
            var rim=Panel("Board rim",stage,Hex("FFF8E9")); Place(rim,0,-153,724,724);
            Shadow(rim.gameObject,Hex("8AB7A3"),new Vector2(0,-12));
            board=Panel("Board",rim,Hex("C4D7DD")); Place(board,0,0,696,696);board.gameObject.AddComponent<RectMask2D>();
            for(int y=0;y<Rows;y++) for(int x=0;x<Columns;x++) {
                var slot=Panel("Slot",board,Hex("B3CBD3")); Place(slot,(x-3)*96,(3-y)*96,88,88);
                cells[x,y]=random.Next(colors.Length); DrawTile(x,y);
            }
            hint=Label("Tap 2 or more matching jellies!",stage,new Vector2(0,-563),new Vector2(710,50),29,Hex("416F67"));
            var bar=Panel("Score track",stage,Hex("A2CFB6")); Place(bar,0,-620,540,22);
            progress=Panel("Score fill",bar,Hex("FFF3AE")); progress.anchorMin=new Vector2(0,.5f);progress.anchorMax=new Vector2(0,.5f);progress.pivot=new Vector2(0,.5f);progress.sizeDelta=new Vector2(15,22);
            scoreLabel=Label("0 / 2000",stage,new Vector2(0,-667),new Vector2(400,42),25,Hex("416F67"));
            Button("Restart",stage,new Vector2(300,383),new Vector2(100,66),"FFF9E9","Reset",20,ResetGame);
            if (EventSystem.current == null) {
                var events = new GameObject("Jelly Garden EventSystem", typeof(EventSystem));
                events.transform.SetParent(transform);
                // Work with either input backend without requiring an Input System dependency.
                var inputType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputType != null) events.AddComponent(inputType); else events.AddComponent<StandaloneInputModule>();
            }
            Layout();
        }
        void Update() {
            if(!busy && board) for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++) {
                float breathe=Mathf.Sin(Time.unscaledTime*2f+x*.8f+y*.6f)*.018f;
                tiles[x,y].localScale=new Vector3(1+breathe,1-breathe,1);
            }
            if (lastSize != new Vector2(Screen.width,Screen.height) || lastSafe != Screen.safeArea) Layout(); }
        void Layout() {
            if(!safe) return;
            lastSize=new Vector2(Screen.width,Screen.height); lastSafe=Screen.safeArea;
            safe.anchorMin=lastSafe.min/lastSize; safe.anchorMax=lastSafe.max/lastSize; safe.offsetMin=safe.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();
            bool landscape=safe.rect.width>safe.rect.height*1.3f;
            float scale=landscape?Mathf.Min(safe.rect.width/1500f,safe.rect.height/900f):Mathf.Min(safe.rect.width/820f,safe.rect.height/1480f);
            stage.localScale=Vector3.one*scale; stage.anchoredPosition=new Vector2(0,landscape?0:30*scale);
            Position("JELLY",landscape?new Vector2(-440,290):new Vector2(0,540));
            Position("G A R D E N",landscape?new Vector2(-440,216):new Vector2(0,466));
            Position("LEVEL 12",landscape?new Vector2(-440,130):new Vector2(0,383));
            Position("Goals",landscape?new Vector2(-440,0):new Vector2(0,270),landscape?.75f:1);
            Position("Board rim",landscape?new Vector2(340,0):new Vector2(0,-153));
            hint.rectTransform.anchoredPosition=landscape?new Vector2(-440,-135):new Vector2(0,-563);
            hint.rectTransform.localScale=Vector3.one*(landscape?.76f:1);
            Position("Score track",landscape?new Vector2(-440,-205):new Vector2(0,-620),landscape?.8f:1);
            scoreLabel.rectTransform.anchoredPosition=landscape?new Vector2(-440,-250):new Vector2(0,-667);
            Position("Restart",landscape?new Vector2(-440,-335):new Vector2(300,383));
        }
        void Position(string name,Vector2 position,float scale=1) {
            var element=(RectTransform)stage.Find(name);element.anchoredPosition=position;element.localScale=Vector3.one*scale;
        }
        Sprite Shape(int radius) {
            const int n=128; var texture=new Texture2D(n,n,TextureFormat.RGBA32,false); texture.name="Jelly procedural shape";
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++) {
                float dx=Mathf.Max(Mathf.Abs(x-63.5f)-(64-radius),0),dy=Mathf.Max(Mathf.Abs(y-63.5f)-(64-radius),0);
                pixels[y*n+x]=new Color(1,1,1,Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)));
            }
            texture.SetPixels(pixels);texture.Apply();
            var sprite=Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius));
            owned.Add(sprite);owned.Add(texture);return sprite;
        }
        RectTransform Rect(string name,Transform parent) { var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);return (RectTransform)g.transform; }
        RectTransform Panel(string name,Transform parent,Color color) { var r=Rect(name,parent);var image=r.gameObject.AddComponent<Image>();image.sprite=round;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return r; }
        void Blob(Transform p,Vector2 pos,Vector2 size,string color) { var r=Panel("Scenery",p,Hex(color));r.GetComponent<Image>().sprite=circle;r.GetComponent<Image>().type=Image.Type.Simple;Place(r,pos.x,pos.y,size.x,size.y); }
        void Stretch(RectTransform r) {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        void Place(RectTransform r,float x,float y,float w,float h) { r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h); }
        Text Label(string value,Transform p,Vector2 pos,Vector2 size,int pixels,Color color) {
            var r=Rect(value,p);Place(r,pos.x,pos.y,size.x,size.y);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=pixels;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.raycastTarget=false;t.fontStyle=FontStyle.Bold;return t;
        }
        void Shadow(GameObject g,Color color,Vector2 distance) {var s=g.AddComponent<Shadow>();s.effectColor=color;s.effectDistance=distance;}
        void Pill(Transform p,string text,Vector2 pos,Vector2 size,string color,int fontSize) {var r=Panel(text,p,Hex(color));Place(r,pos.x,pos.y,size.x,size.y);Label(text,r,Vector2.zero,size,fontSize,Color.white);}
        void Button(string name,Transform p,Vector2 pos,Vector2 size,string color,string text,int fontSize,UnityEngine.Events.UnityAction action) {
            var r=Panel(name,p,Hex(color));Place(r,pos.x,pos.y,size.x,size.y);r.GetComponent<Image>().raycastTarget=true;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(action);Label(text,r,Vector2.zero,size,fontSize,Hex("62739A"));
        }
        RectTransform Jelly(Transform p,int color,Vector2 pos,float size) {
            var r=Panel("Jelly",p,colors[color]);Place(r,pos.x,pos.y,size,size);
            Shadow(r.gameObject,Color.Lerp(colors[color],Hex("526078"),.35f),new Vector2(0,-5));
            var shine=Panel("Shine",r,new Color(1,1,1,.38f));Place(shine,-size*.12f,size*.29f,size*.48f,size*.09f);
            for(int eye=0;eye<2;eye++) {var e=Panel("Eye",r,Hex("51536F"));e.GetComponent<Image>().sprite=circle;Place(e,(eye==0?-.16f:.16f)*size,-size*.02f,size*.075f,size*.1f);}
            var smile=Panel("Smile",r,new Color(.32f,.33f,.43f,.55f));Place(smile,0,-size*.17f,size*.12f,size*.045f);
            return r;
        }
        void DrawTile(int x,int y) {
            var r=Jelly(board,cells[x,y],new Vector2((x-3)*96,(3-y)*96+3),78);tiles[x,y]=r;r.name="Jelly "+x+","+y;
            var image=r.GetComponent<Image>();image.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;BindTile(x,y);
        }
        void BindTile(int x,int y) {
            var tile=tiles[x,y];tile.name="Jelly "+x+","+y;
            tile.SetAsLastSibling(); // Falling tiles must stay above every slot background.
            var button=tile.GetComponent<Button>();button.onClick.RemoveAllListeners();
            button.onClick.AddListener(()=>Tap(x,y));
        }
        List<Vector2Int> Group(int x,int y) {
            var found=new List<Vector2Int>(); var pending=new Queue<Vector2Int>();pending.Enqueue(new Vector2Int(x,y));
            while(pending.Count>0){var p=pending.Dequeue();if(p.x<0||p.y<0||p.x>=Columns||p.y>=Rows||found.Contains(p)||cells[p.x,p.y]!=cells[x,y])continue;found.Add(p);pending.Enqueue(p+Vector2Int.up);pending.Enqueue(p+Vector2Int.down);pending.Enqueue(p+Vector2Int.left);pending.Enqueue(p+Vector2Int.right);}
            return found;
        }
        public void Tap(int x,int y) {
            if(busy||moves<=0||Won||x<0||y<0||x>=Columns||y>=Rows)return;
            var group=Group(x,y);if(group.Count<2){hint.text="Find a matching pair!";return;}StartCoroutine(Pop(group));
        }
        public void PlayBestMove() {
            if(busy)return;List<Vector2Int> best=null;
            for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++){var g=Group(x,y);if(best==null||g.Count>best.Count)best=g;}
            if(best.Count>=2)Tap(best[0].x,best[0].y);else ResetGame();
        }
        IEnumerator Pop(List<Vector2Int> group) {
            busy=true;moves--;score+=group.Count*60;
            int color=cells[group[0].x,group[0].y];
            if(color<3){targets[color]=Mathf.Max(0,targets[color]-group.Count);targetLabels[color].text=targets[color]==0?"OK":targets[color].ToString();}movesLabel.text=moves.ToString();
            hint.text=group.Count>=5?"SWEET!  +"+group.Count*60:"Lovely!  +"+group.Count*60;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime*3.5f){foreach(var p in group)tiles[p.x,p.y].localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.2f)*(1-t*t);yield return null;}
            foreach(var p in group){Destroy(tiles[p.x,p.y].gameObject);cells[p.x,p.y]=-1;}
            for(int x=0;x<Columns;x++){
                int bottom=Rows-1;
                for(int y=Rows-1;y>=0;y--)if(cells[x,y]>=0){cells[x,bottom]=cells[x,y];tiles[x,bottom]=tiles[x,y];bottom--;}
                for(int y=bottom;y>=0;y--){cells[x,y]=random.Next(colors.Length);DrawTile(x,y);tiles[x,y].anchoredPosition+=Vector2.up*300;}
            }
            for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++)BindTile(x,y);
            var starts=new Vector2[Columns,Rows];for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++)starts[x,y]=tiles[x,y].anchoredPosition;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime*2.6f){for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++)tiles[x,y].anchoredPosition=Vector2.Lerp(starts[x,y],new Vector2((x-3)*96,(3-y)*96+3),1-Mathf.Pow(1-t,3));yield return null;}
            for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++)tiles[x,y].anchoredPosition=new Vector2((x-3)*96,(3-y)*96+3);
            scoreLabel.text=score+" / 2000";progress.sizeDelta=new Vector2(540*Mathf.Clamp01(score/2000f),22);
            hint.text=Won?"GARDEN COMPLETE!  Tap Reset to play again":moves==0?"Out of moves!  Tap Reset to try again":"Tap 2 or more matching jellies!";busy=false;
        }
        public void ResetGame() {
            StopAllCoroutines();busy=false;score=0;moves=24;random=new System.Random(42);
            targets[0]=18;targets[1]=12;targets[2]=15;for(int i=0;i<3;i++)targetLabels[i].text=targets[i].ToString();
            for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++){Destroy(tiles[x,y].gameObject);cells[x,y]=random.Next(colors.Length);DrawTile(x,y);}
            movesLabel.text="24";scoreLabel.text="0 / 2000";progress.sizeDelta=new Vector2(15,22);hint.text="Tap 2 or more matching jellies!";
        }
        void OnDestroy(){foreach(var asset in owned)if(asset)Destroy(asset);}
    }
}
