using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class GeroBeam : MonoBehaviour {
	public GameObject HitEffect;
	private ShotParticleEmitter SHP_Emitter;

	private float NowLength;
	public float MaxLength = 16.0f;
	public float AddLength = 0.1f;
	public float Width = 10.0f;
	private LineRenderer LR;
	private Vector3[] F_Vec;
	private int LRSize;
	private GeroBeamHit HitObj;
	private float RateA;

	public float NowLengthGlobal;
	private BeamParam BP;
    private Vector3 HitObjSize;
    private GameObject Flash;
    private float FlashSize;

    public LayerMask laserLayerMask;
    // Use this for initialization
    void Start () {
		BP = GetComponent<BeamParam>();
		LRSize = 16;
		NowLength = 0.0f;
		LR = this.GetComponent<LineRenderer>();
		HitObj = this.transform.Find("GeroBeamHit").GetComponent<GeroBeamHit>();
        HitObjSize = HitObj.transform.localScale;
        SHP_Emitter = this.transform.Find("ShotParticle_Emitter").GetComponent<ShotParticleEmitter>();
        Flash = this.transform.Find("BeamFlash").gameObject;
        F_Vec = new Vector3[LRSize+1];
        FlashSize = Flash.transform.localScale.x;
        for (int i=0;i < LRSize+1;i++)
		{
			F_Vec[i] = transform.forward;
		}

        Width = 0;
        DisableBeam();
    }

    [Button]
    public void ActivateBeam() {
	    gameObject.SetActive(true);
	    BP.bEnd = false;
	    SHP_Emitter.ShotPower = 1;
    }

    [Button]
    public void DisableBeam() {
	    BP.bEnd = true;
    }
	
	// Update is called once per frame
	public int laserRefreshSpeed = 3;
	void Update () {
        if (BP.bEnd && BP.bGero) {
	        Width -= 1f * Time.deltaTime;
			SHP_Emitter.ShotPower = 0.0f;
			if (Width < 0.01f) {
	            BP.bGero = false;
	            Width = 0;
	            
	            gameObject.SetActive(false);
	            
            }
		}else if (!BP.bEnd && !BP.bGero) {
	        if (Width < 1f) {
		        Width += 1f * Time.deltaTime;
		        SHP_Emitter.ShotPower = 1;
	        } else {
		        BP.bGero = true;
		        Width = 1;
	        }
        }


        if (BP.bGero || !BP.bEnd) {
	        NowLength = Mathf.Min(1.0f, NowLength + AddLength);

	        Vector3 NowPos = Vector3.zero;

	        LR.SetWidth(Width * BP.Scale, Width * BP.Scale);
	        LR.SetColors(BP.BeamColor, BP.BeamColor);
	        MaxLength = BP.MaxLength;
	        
	        for (int m = 0; m < laserRefreshSpeed; m++) {
		        for (int i = LRSize - 1; i > 0; i--) {
			        F_Vec[i] = F_Vec[i - 1];
		        }

		        F_Vec[0] = transform.forward;
		        F_Vec[LRSize] = F_Vec[LRSize - 1];
	        }

	        float BlockLen = MaxLength / LRSize;

	        for (int i = 0; i < LRSize; i++) {
		        NowPos = transform.position;
		        for (int j = 0; j < i; j++) {
			        NowPos += F_Vec[j] * BlockLen;
		        }

		        LR.SetPosition(i, NowPos);
	        }

	        //Collision
	        bool bHitNow = false;
	        float workNLG = 1.0f;
	        NowLengthGlobal = NowLength * LRSize;

	        if (Width >= 0.01f) {
		        for (int i = 0; i < LRSize; i++) {
			        workNLG = Mathf.Min(1.0f, NowLengthGlobal - i);

			        NowPos = transform.position;
			        for (int j = 0; j < i; j++) {
				        NowPos += F_Vec[j] * BlockLen;
			        }


			        RaycastHit hit;
			        if (workNLG <= 0)
				        break;
			        if (Physics.Raycast(NowPos, F_Vec[i], out hit, BlockLen * workNLG, laserLayerMask)) {
				        GameObject hitobj = hit.collider.gameObject;
				        NowLength = ((BlockLen * i) + hit.distance) / MaxLength;
				        HitObj.transform.position = NowPos + F_Vec[i] * hit.distance;
				        HitObj.transform.rotation = Quaternion.AngleAxis(180.0f, transform.up) * this.transform.rotation;
				        //HitObj.transform.localScale = HitObjSize * Width * BP.Scale * 10.0f;
				        bHitNow = true;
				        break;
			        }
		        }
	        }

	        float ShotFlashScale = FlashSize * Width * 5.0f * BP.Scale;
	        var scaleVec = new Vector3(ShotFlashScale, ShotFlashScale, ShotFlashScale);
	        Flash.GetComponent<ScaleWiggle>().DefScale = scaleVec;
	        HitObj.SetViewPat(bHitNow && !BP.bEnd, scaleVec);

	        this.gameObject.GetComponent<Renderer>().material.SetFloat("_AddTex", Time.frameCount * -0.05f * BP.AnimationSpd * 10);
	        this.gameObject.GetComponent<Renderer>().material.SetFloat("_BeamLength", NowLength+0.01f);
	        Flash.GetComponent<Renderer>().material.SetColor("_Color", BP.BeamColor * 2);
	        SHP_Emitter.col = BP.BeamColor * 2;
	        HitObj.col = BP.BeamColor * 2;
        }
	}
}
