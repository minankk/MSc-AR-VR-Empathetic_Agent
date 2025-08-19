using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using DG.Tweening;
//using OVRSimpleJSON;

/*
- installling Newtonsoft Json Unity Package:     https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM
- Need LeanTween package
*/


/*
https://imotions.com/blog/learning/research-fundamentals/facial-action-coding-system/

Categorical emotions: 
Happiness 6 + 12
Sadness	1 + 4 + 15
Surprise	1 + 2 + 5 + 26
Fear	1 + 2 + 4 + 5 + 7 + 20 + 26
Anger	4 + 5 + 7 + 23
Disgust	9 + 15 + 16
Contempt	12 + 14

*/

public struct BoneRoation {
    public BoneRoation(HumanBodyBones hbb, Vector3 vNeutral, Vector3 vRotation) {
        Bone = hbb;
        Neutral = vNeutral;
        Rotation = vRotation;
    }

    public HumanBodyBones Bone;
    public Vector3 Neutral { get; }
    public Vector3 Rotation { get; }
}

public class FaceController : MonoBehaviour {
    

    public string FaceMeshName = "CC_Base_Body";
    public string mappingFile = "AU2BS-CC3+.json";
    public string weightFile = "male_faces.json";



    Dictionary<string, List<string>> dictAU2Blendshapes; // Mapping Action Units to Blendshapes
    Dictionary<string, List<float>> dictAU2BlendshapeWeights; // Allows fine-tuning the weight of the mapping where necessary
    Dictionary<string, List<float>> dictPADWeights; // Mapping of P, A, D to Action Unit 
    Dictionary<string, List<int>> dictCategoricalEmotions; // Composition of categorical emotions from Action Units

    Dictionary<string, BoneRoation> dictAU2BoneRotation; // Mapping Action Units to Bones

    private string strCurrentEmotion;
    Animator anim;

    Dictionary<HumanBodyBones, Vector3> dictBonesToRotate; // dictionary to hold temorary rotation values
    List<Tuple<int, float>> lstIndexWeights = new List<Tuple<int, float>>();
    SkinnedMeshRenderer skinnedMeshRendererFace;

    private void Awake() {
        Debug.Log("FaceController:Awake()");
        try {
            weightFile = weightFile.Replace(".json", "");
            string strPADWeights = Resources.Load<TextAsset>(Path.Combine("FaceConfigs/", weightFile)).text;
            dictPADWeights = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(strPADWeights);
        } catch (Exception ex) {
            DebugDisplay.Instance.AddText("ERROR loading Weight file resource: " + ex.ToString());
        }

        try {
            mappingFile = mappingFile.Replace(".json", "");
            string strAUMapping = Resources.Load<TextAsset>(Path.Combine("FaceConfigs/", mappingFile)).text;
            dictAU2Blendshapes = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(strAUMapping);
        } catch (Exception ex) {
            DebugDisplay.Instance.AddText("ERROR loading Blendshape mapping resouce: " + ex.ToString());
        }


        // This disctionary allows to tune the weight of the Blendshape e.g. dictAU2BlendshapeWeights.Add("au_10", new List<float> { 0.6f, 0.6f });
        dictAU2BlendshapeWeights = new Dictionary<string, List<float>>();


        // some corrections
        dictAU2BlendshapeWeights.Add("25", new List<float> { 0.5f, 0.5f, 0.5f, 0.5f });
        dictAU2BlendshapeWeights.Add("22", new List<float> { 0.5f, 0.5f, 0.5f, 0.5f });
        dictAU2BlendshapeWeights.Add("17", new List<float> { 1.5f });

        dictCategoricalEmotions = new Dictionary<string, List<int>>();
        dictCategoricalEmotions.Add("Happiness", new List<int>() { 6, 12 });
        dictCategoricalEmotions.Add("Sadness", new List<int>() { 1, 4, 15 });
        dictCategoricalEmotions.Add("Surprise", new List<int>() { 1, 2, 5, 26 });
        dictCategoricalEmotions.Add("Fear", new List<int>() { 1, 2, 4, 5, 7, 20, 26 });
        dictCategoricalEmotions.Add("Anger", new List<int>() { 4, 5, 7, 23 });
        dictCategoricalEmotions.Add("Disgust", new List<int>() { 9, 15, 16 });
        dictCategoricalEmotions.Add("Contempt", new List<int>() { 12, 14 });

        dictAU2BoneRotation = new Dictionary<string, BoneRoation>();


        BoneRoation br =
            new BoneRoation(//"CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02/CC_Base_NeckTwist01/CC_Base_NeckTwist02/CC_Base_Head/CC_Base_FacialBone/CC_Base_JawRoot", 
            HumanBodyBones.Jaw,
            new Vector3(0f, 0f, -90f),
            new Vector3(-0f, 0f, -95f)); // original: -110
        dictAU2BoneRotation.Add("26", br);


        dictBonesToRotate = new Dictionary<HumanBodyBones, Vector3>();

        anim = GetComponent<Animator>();

        GameObject go = this.transform.Find(FaceMeshName).gameObject;
        skinnedMeshRendererFace = go.GetComponent<SkinnedMeshRenderer>();
    }

    private void Start() {
        // printBlendShapeNames(strFaceMesh);
        //setCategoricalEmotion("Happiness", 1.0f);
        //setCategoricalEmotion("Anger", 1.0f);
        //setPAD2AUNorm(-1.0f, 0.0f, 1.0f);
        // printdictAU2Blendshapes();

        // StartCoroutine(ScanPAD());
        // StartCoroutine(showAllAUs());
        //StartCoroutine(ScanCategorical());
    }


    /// <summary>
    /// Control via pleasure-arousal-dominance
    /// Values in range -1...1
    /// </summary>
    /// <param name="pleasure"></param>
    /// <param name="arousal"></param>
    /// <param name="dominance"></param>
    public void setPAD2AUNorm(float pleasure, float arousal, float dominance) {
//        resetAUs();

        //Debug.Log("------------");
        //Debug.Log(pleasure);
        //Debug.Log("pleasure: " + pleasure.ToString() + ", arousal: " + arousal.ToString() + ", dominance: " + dominance);

        /* what comes in is 0...1, but we need -100..100 for the model */
        pleasure = (pleasure * 200.0f) - 100.0f;
        arousal = (arousal * 200.0f) - 100.0f;
        dominance = (dominance * 200.0f) - 100.0f;
        //Debug.Log("pleasure: " + pleasure.ToString() + ", arousal: " + arousal.ToString() + ", dominance: " + dominance);


        foreach (KeyValuePair<string, List<float>> entry in dictPADWeights) {
            List<float> lstWeights = entry.Value;
            float p = pleasure * lstWeights[0];
            float a = arousal * lstWeights[1];
            float d = dominance * lstWeights[2];
            float AUWeight = p + a + d;
            AUWeight *= .2f; // "random" correction factor


            //Debug.Log("\t AU: " + entry.Key + " w0: " + lstWeights[0].ToString() + ", w1: " + lstWeights[1].ToString() + ", w2: " + lstWeights[2].ToString());
            //Debug.Log("\t" + " p: " + p.ToString() + ", a: " + a.ToString() + ", d: " + d.ToString() + " AUWeight: " + AUWeight.ToString());

            AUWeight = AUWeight > 0 ? AUWeight : 0;
            AUWeight = AUWeight < 1 ? AUWeight : 1;

            setAU(entry.Key, "both", AUWeight);
        }
    }


    public void setPAD2AUNorm(float pleasure, float arousal, float dominance, float timeIn, float timeHold, float timeOut) {
        //Debug.Log("setPAD2AUNorm p:" + pleasure.ToString() + ", a: " + arousal.ToString() + ", d: " + dominance.ToString() + " " + timeIn.ToString() + " " + timeHold.ToString() + " " + timeOut.ToString());
        //resetAUs();
        // new need to tween p, a, d separately

        /* 
        LeanTween.value(gameObject, tweenPAD, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(pleasure, arousal, dominance), timeIn).setEase(LeanTweenType.easeInExpo);
        LeanTween.value(gameObject, tweenPAD, new Vector3(pleasure, arousal, dominance), new Vector3(0.5f, 0.5f, 0.5f), timeOut).setDelay(timeHold).setEase(LeanTweenType.easeOutExpo);
        */


        /* NEEDS TESTING...*/
        Vector3 v3pad = new Vector3(0.5f, 0.5f, 0.5f);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(DOTween.To(() => v3pad, x => v3pad = x, new Vector3(pleasure, arousal, dominance), timeIn).SetEase(Ease.InExpo).OnUpdate(() => tweenPAD(v3pad)));
        mySequence.AppendInterval(timeHold);
        mySequence.Append(DOTween.To(() => v3pad, x => v3pad = x, new Vector3(0.5f, 0.5f, 0.5f), timeOut).SetEase(Ease.InExpo).OnUpdate(() => tweenPAD(v3pad)));

    }

    public void setCategoricalEmotion(string strEmotionName, float fWeight) {
        //Debug.LogWarning("FaceController::setCategoricalEmotion");
        // resetAUs();
        if (dictCategoricalEmotions.ContainsKey(strEmotionName)) {
            List<int> lstAUs = dictCategoricalEmotions[strEmotionName];
            foreach (int AU in lstAUs) {
                setAU(AU.ToString(), "both", fWeight);
            }
        }
    }

    public void ClearBoneRotations(){
        dictBonesToRotate.Clear();
        resetAUs();
    }

    public void setCategoricalEmotion(string strEmotionName, float fWeight, float timeIn, float timeHold, float timeOut) {
        Debug.LogWarning("setCategoricalEmotion " + strEmotionName + " " + fWeight.ToString() + " " + timeIn.ToString() + " " + timeHold.ToString() + " " + timeOut.ToString());
        //LogManager.instance.WriteTimeStampedEntry("FaceController::setCategoricalEmotion:" + strEmotionName + ":" + fWeight.ToString() + ":" + timeIn.ToString() + ":" + timeHold.ToString() + ":" + timeOut.ToString());
        strCurrentEmotion = strEmotionName;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(DOTween.To(tweenCategorical, 0.0f, fWeight, timeIn).SetEase(Ease.OutExpo));
        mySequence.AppendInterval(timeHold);
        mySequence.Append(DOTween.To(tweenCategorical, fWeight, 0.0f, timeIn).SetEase(Ease.OutExpo).OnComplete(ClearBoneRotations));
    }

    public void setCategoricalEmotion(string strEmotionName, float fWeight, float timeIn, float timeHold, float timeOut, float delay) {
        //Debug.LogWarning("setCategoricalEmotion " + strEmotionName + " " + fWeight.ToString() + " " + timeIn.ToString() + " " + timeHold.ToString() + " " + timeOut.ToString());
        //LogManager.instance.WriteTimeStampedEntry("FaceController::setCategoricalEmotion:" + strEmotionName + ":" + fWeight.ToString() + ":" + timeIn.ToString() + ":" + timeHold.ToString() + ":" + timeOut.ToString() + ":" + delay.ToString());
        strCurrentEmotion = strEmotionName;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(DOTween.To(tweenCategorical, 0.0f, fWeight, timeIn).SetEase(Ease.OutExpo));
        mySequence.AppendInterval(timeHold);
        mySequence.Append(DOTween.To(tweenCategorical, fWeight, 0.0f, timeIn).SetEase(Ease.OutExpo).OnComplete(ClearBoneRotations));
        mySequence.PrependInterval(delay);
    }


    /// <summary>
    /// Weights are 0.0f...1.0f
    /// </summary>
    /// <param name="strAU"></param>
    /// <param name="strSide"></param>
    /// <param name="fWeight"></param>
    public void setAU(string strAU, string strSide, float fWeight) {
        //Debug.Log("CharCtrlFAU::setAU:" + strAU + ":" + strSide + ":" + fWeight);

        if (strSide.ToLower() == "both") { strSide = ""; }

        string strSideSuffix = (strSide.Length > 0 ? "_" + strSide.ToLower() : "");
        string strKey = strAU + strSideSuffix;

        List<string> lstBlendshapes = new List<string>();
        if (dictAU2Blendshapes.ContainsKey(strKey)) {
            lstBlendshapes = dictAU2Blendshapes[strKey];
        } else {
            // Debug.Log("CharCtrlFAU::setAU: AU " + strKey + " not found");
        }

        List<float> lstBlendshapeWeight = null;
        if (dictAU2BlendshapeWeights.ContainsKey(strKey)) {
            lstBlendshapeWeight = dictAU2BlendshapeWeights[strKey];
        }

        int ii = 0;
        foreach (string ss in lstBlendshapes) {
            float w = fWeight;

            if (lstBlendshapeWeight != null) {
                w *= lstBlendshapeWeight[ii];
            }
            setMeshBlendshapeWeightCache(ss, w * 100.0f);
            ii++;
        }

        /* BONES */
        if (dictAU2BoneRotation.ContainsKey(strKey)) {
            //Debug.Log("AU for Bone: " + strKey);
            BoneRoation br = dictAU2BoneRotation[strKey];
            Vector3 newRot = Vector3.Lerp(br.Neutral, br.Rotation, fWeight);
            dictBonesToRotate[br.Bone] = newRot;
        }
    }

    void OnAnimatorIK(int layerIndex) {
       /* work through the list of bones
       The dictionary is cleared at the end of the tween cycle
       */

        foreach (KeyValuePair<HumanBodyBones, Vector3> bb in dictBonesToRotate){
            anim.SetBoneLocalRotation(bb.Key, Quaternion.Euler(bb.Value));
        }

    }



    public void resetAUs() {
        /* we only reset the AUs, not all the Blenshapes to avoid interference with other Blendshape alterations */
        foreach (KeyValuePair<string, List<string>> entry in dictAU2Blendshapes) {
            List<string> lstBlendshapes = entry.Value;
            foreach (string strBS in lstBlendshapes) {
                setMeshBlendshapeWeightCache(strBS, 0.0f);
            }
        }
    }


    private void tweenCategorical(float val) {
        //Debug.Log("tweened value:" + val);
        setCategoricalEmotion(strCurrentEmotion, val);
    }


     private void tweenPAD(Vector3 vVal) {
        //Debug.Log("tweened value:" + vVal);
        setPAD2AUNorm(vVal[0], vVal[1], vVal[2]);
    }


    public void setMeshBlendshapeWeight(string strBlendshape, float fWeight) {
        //Debug.Log("CharCtrlFAU::setMeshBlendshapeWeight:" + strMesh + ":" + strBlendshape + ":" + fWeight);
        Mesh skinnedMesh = skinnedMeshRendererFace.sharedMesh;
        int index = skinnedMesh.GetBlendShapeIndex(strBlendshape);
        if (index >= 0) {
            skinnedMeshRendererFace.SetBlendShapeWeight(index, fWeight);
        }
    }

    /// <summary>
    /// This function does not directly set the blendshape weights, but creates a list that is then iterated in LateUpdate. This is necessary when using Animancer AnimationControllers
    /// </summary>
    /// <param name="strMesh"></param>
    /// <param name="strBlendshape"></param>
    /// <param name="fWeight"></param>
    public void setMeshBlendshapeWeightCache(string strBlendshape, float fWeight) {
        //Debug.Log("CharCtrlFAU::setMeshBlendshapeWeightCache:" + strMesh + ":" + strBlendshape + ":" + fWeight);
        Mesh skinnedMesh = skinnedMeshRendererFace.sharedMesh;
        int index = skinnedMesh.GetBlendShapeIndex(strBlendshape);
        if (index >= 0) {
            skinnedMeshRendererFace.SetBlendShapeWeight(index, fWeight);
            lstIndexWeights.Add(new Tuple<int, float>(index, fWeight));
        }
    }


    public void resetMeshBlendshapeWeight() {
        Mesh skinnedMesh = skinnedMeshRendererFace.sharedMesh;

        for (int ii = 0; ii < skinnedMesh.blendShapeCount; ii++) {
            skinnedMeshRendererFace.SetBlendShapeWeight(ii, 0);
        }
    }

    // -------------------------------------------
    // Functions to explore the facial expressions


    private void printdictAU2Blendshapes() {
        //resetAUs();
        foreach (KeyValuePair<string, List<string>> entry in dictAU2Blendshapes) {
            Debug.Log(entry.Key);
            List<string> lstBS = entry.Value;
            foreach (string BS in lstBS) {
                Debug.Log("\t" + BS);
            }
        }
    }

    IEnumerator showAllAUs() {
        foreach (KeyValuePair<string, List<string>> entry in dictAU2Blendshapes) {
            string strAU = entry.Key;
            Debug.Log("AU: " + strAU);
            resetAUs();
            setAU(strAU, "both", 1.0f);
            strAU = (strAU.Length > 1 ? strAU : "0" + strAU);
            string strFN = this.name + "_" + strAU + ".png";
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.streamingAssetsPath, strFN));
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void printBlendShapeNames(string strMesh) {
        string path = "Assets/Temp/" + this.name + "_Blendshapes.txt";
        StreamWriter writer = new StreamWriter(path, true);
        Mesh skinnedMesh = skinnedMeshRendererFace.sharedMesh;
        for (int i = 0; i < skinnedMesh.blendShapeCount; i++) {
            string s = skinnedMesh.GetBlendShapeName(i);
            //            Debug.Log("Blend Shape: " + i + " " + s);
            //writer.WriteLine(s + "(" + i + ")");
            writer.WriteLine(s);
        }
        writer.Close();
    }

    IEnumerator ScanPAD() {
        float a = 0f;
        float r = 1.0f; // Replace with the desired value for 'r'.
        float fDom = 1.0f; // Replace with the desired value for 'fDom'.

        while (a < (2 * Math.PI)) {
            double adeg = a * 180 / Math.PI;
            float fAro = Mathf.Sin(a) * r;
            float fPle = Mathf.Cos(a) * r;
            string idstr = fPle.ToString() + "_" + fAro.ToString() + "_" + fDom.ToString();
            Debug.Log("************ a: " + adeg + ", Aro: " + fAro + ", Val: " + fPle);
            resetAUs();
            setPAD2AUNorm(fPle, fAro, fDom);
            a += (Mathf.PI / 8);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator ScanCategorical() {
        foreach (KeyValuePair<string, List<int>> cat in dictCategoricalEmotions) {
            Debug.Log("Emotion: " + cat.Key);
            setCategoricalEmotion(cat.Key, 1.0f);
            yield return new WaitForSeconds(5);
        }
    }

    private void LateUpdate() {
        foreach (Tuple<int, float> tuple in lstIndexWeights) {
            skinnedMeshRendererFace.SetBlendShapeWeight(tuple.Item1, tuple.Item2);

        }

    }

}

