using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A single, predictable input surface aligned with the actual 1280x720
// approved reference picture. It intentionally replaces nested invisible
// raycast hitboxes while preserving the existing scoring and puzzle methods.
// Touch and mouse both use Unity UI pointer events (Android safe).
public sealed class Stage5ReferenceTouchController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Stage5ElementCrucibleGame game;
    private Stage5ReferenceLayout skin;
    private RectTransform surface;
    private Image ghost;
    private int level, source=-1;
    private int downAction=-1;
    private bool adjustingHeat;
    private Vector2 start;
    private bool initialized;

    public void Setup(Stage5ElementCrucibleGame owner,Stage5ReferenceLayout renderer,int stageLevel)
    {
        game=owner;
        skin=renderer;
        level=stageLevel;
        var rect=GetComponent<RectTransform>();
        surface=rect;
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
        rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
        var img=GetComponent<Image>();
        if(img==null)img=gameObject.AddComponent<Image>();
        img.color=Color.clear;
        img.raycastTarget=true;
        initialized=true;
    }

    private bool Local(PointerEventData e,out Vector2 point)
    {
        point=Vector2.zero;
        if(!initialized||surface==null)return false;
        var canvas=surface.GetComponentInParent<Canvas>();
        var camera=canvas!=null && canvas.renderMode!=RenderMode.ScreenSpaceOverlay?
            e.pressEventCamera:null;
        // Use canvas coordinates so all 16:9 reference artwork targets have
        // the same hit locations on Windows editor and Android landscape.
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            surface,e.position,camera,out point))return false;
        // Normalize to the artwork's 1280x720 logical coordinates, even on
        // wider Android devices with a 19.5:9 or 20:9 physical screen.
        var size=surface.rect.size;
        if(size.x<=0||size.y<=0)return false;
        point=new Vector2(point.x*1280f/size.x,point.y*720f/size.y);
        return true;
    }

    private static bool Contains(Vector2 p,float x0,float y0,float x1,float y1)
    {
        return p.x>=x0 && p.x<=x1 && p.y>=y0 && p.y<=y1;
    }

    private int SampleAt(Vector2 p)
    {
        // Coordinates are centered on the full 1280x720 artwork.
        if(p.x< -530 || p.x> -270)return -1;
        if(p.y>=10 && p.y<=92)return 0;
        if(p.y>=-66 && p.y<10)return 1;
        if(p.y>=-145 && p.y< -66)return 2;
        return -1;
    }

    private int BinAt(Vector2 p)
    {
        if(level!=3||p.y> -276||p.y< -343)return -1;
        if(p.x>=-246&&p.x<=-2)return 0;  // Ionic
        if(p.x>=7&&p.x<=277)return 1;    // Covalent
        if(p.x>=285&&p.x<=540)return 2;  // Metallic
        return -1;
    }

    private bool BenchAt(Vector2 p)
    {
        // Entire illustrated experiment workbench, not the buttons below.
        return p.x>=-270&&p.x<=280&&p.y>=-166&&p.y<=155;
    }

    private bool HeatSliderAt(Vector2 p)
    {
        return level==1&&Contains(p,-312,-258,289,-188);
    }

    private void HeatPointer(Vector2 p)
    {
        float value=Mathf.Clamp01(Mathf.InverseLerp(-300,280,p.x))*100f;
        game.ReferenceSetHeat(value);
        skin.ReferenceHeatChanged(value);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if(!game.CanInteract)return;
        if(!Local(e,out var p))return;
        start=p;source=SampleAt(p);adjustingHeat=false;downAction=-1;
        if(source>=0)
        {
            ShowGhost(source,p);
            return;
        }
        if(HeatSliderAt(p))
        {
            adjustingHeat=true;
            HeatPointer(p);
            return;
        }
        downAction=ActionAt(p);
    }

    private int ActionAt(Vector2 p)
    {
        if(Contains(p,-640,270,-545,360))return 50; // menu
        if(level==1)
        {
            if(Contains(p,-535,-333,-175,-255))return 0; // heat
            if(Contains(p,172,-333,540,-255))return 4; // submit
        }
        else if(level==2)
        {
            // The buttons are centered around y=-245 in the art. The
            // previous -255 upper edge cut off taps near their label centers.
            // Wider hit regions work with both mouse and Android finger taps.
            if(Contains(p,-537,-337,-174,-215))return 1; // strike
            if(Contains(p,-174,-337,174,-215))return 2; // press
            if(Contains(p,173,-337,545,-215))return 4; // submit
        }
        else if(level==3)
        {
            if(Contains(p,-548,-345,-265,-272))return 3; // test probe
            if(Contains(p,-249,-346,-4,-268))return 10; // ionic bin
            if(Contains(p,0,-346,280,-268))return 11; // molecule bin
            if(Contains(p,285,-346,548,-268))return 12; // metal bin
            // Match the independent PhaseButton_* GameObjects in
            // Stage5Level3ModularUI (center x=-200, -34, 132).
            if(Contains(p,-278,-194,-122,-108))return 20; // solid
            if(Contains(p,-112,-194,44,-108))return 21; // molten
            if(Contains(p,54,-194,210,-108))return 22; // aqueous
            if(Contains(p,265,-262,538,-191))return 4; // visible check button
        }
        return -1;
    }

    public void OnDrag(PointerEventData e)
    {
        if(!game.CanInteract)return;
        if(!Local(e,out var p))return;
        if(adjustingHeat)
        {
            HeatPointer(p);
            return;
        }
        if(source>=0 && ghost!=null)
        {
            ghost.rectTransform.anchoredPosition=ToCanvas(p);
            // Visible as soon as the student actually drags beyond a very
            // small tap tolerance, including over the sample bank itself.
            ghost.gameObject.SetActive((p-start).sqrMagnitude>16f);
            ghost.transform.SetAsLastSibling();
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        if(!initialized)return;
        if(!Local(e,out var p)){ResetTouch();return;}
        if(game.CanInteract)
        {
            if(source>=0)
            {
                Debug.Log("CHEMQUEST Stage5 touch: sample "+(char)('A'+source)+" released at "+p);
                int bin=BinAt(p);
                if(bin>=0)
                    game.DropSample(source,bin);
                else if(BenchAt(p)||Vector2.Distance(start,p)<28f)
                    game.DropSample(source,-1);
                // Dropped anywhere else: the original sample stays in place.
            }
            else if(!adjustingHeat)
            {
                var action=ActionAt(p);
                if(action>=0 && action==downAction)
                {
                    Debug.Log("CHEMQUEST Stage5 touch: action "+action+" pressed");
                    if(action>=10&&action<=12)
                        game.ReferencePlaceSelectedInBin(action-10);
                    else if(action>=20&&action<=22)
                    {
                        game.ReferenceChoosePhase(action-20);
                        skin.ReferencePhaseSelected(action-20);
                    }
                    else
                    {
                        game.ReferenceAction(action);
                        // Highlight only a successfully tapped STRIKE or PRESS.
                        // The new border is decorative and does not intercept UI events.
                        if(skin!=null && level==2 && (action==1 || action==2))
                            skin.HighlightAction(action);
                    }
                }
            }
        }
        ResetTouch();
    }

    private void ShowGhost(int sample,Vector2 p)
    {
        if(ghost==null)
        {
            var go=new GameObject("ActualDraggedSamplePreview",typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            // The opaque Stage5Level3_ModularUI is a later Canvas sibling.
            // A ghost nested under this transparent hit surface would render
            // BEHIND that panel and disappear. Parent it directly to Canvas
            // and keep it above the visual UI (while non-raycasting).
            var root=GetComponentInParent<Canvas>();
            go.transform.SetParent(root!=null?root.transform:transform,false);
            ghost=go.GetComponent<Image>();
            ghost.raycastTarget=false;
            ghost.preserveAspect=true;
            ghost.rectTransform.anchorMin=ghost.rectTransform.anchorMax=new Vector2(.5f,.5f);
            ghost.rectTransform.sizeDelta=new Vector2(88,88);
        }
        var sprite=Resources.Load<Sprite>("ChemQuestStage5Reference/DragSample"+sample);
        if(sprite==null)
        {
            var texture=Resources.Load<Texture2D>("ChemQuestStage5Reference/DragSample"+sample);
            if(texture!=null)
                sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),
                    new Vector2(.5f,.5f),100f);
        }
        ghost.sprite=sprite;
        ghost.color=Color.white;
        ghost.type=Image.Type.Simple;
        ghost.preserveAspect=true;
        ghost.raycastTarget=false;
        ghost.rectTransform.localScale=Vector3.one;
        // Guarantee that the sample is in front of the matte and all
        // individual sample/phase/action images as the drag begins.
        ghost.transform.SetAsLastSibling();
        ghost.rectTransform.anchoredPosition=ToCanvas(p);
        ghost.gameObject.SetActive(false);
    }

    private Vector2 ToCanvas(Vector2 reference)
    {
        var size=surface.rect.size;
        return new Vector2(reference.x*size.x/1280f,reference.y*size.y/720f);
    }

    private void ResetTouch()
    {
        source=-1;downAction=-1;adjustingHeat=false;
        if(ghost!=null)ghost.gameObject.SetActive(false);
    }
}
