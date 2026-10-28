using System.Collections.Generic;
using UnityEngine;
namespace DragonValley
{
    public class AudioManager : MonoBehaviour
    {
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        AudioSource source;
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>(); source.volume=.2f;
            string[] keys={"interact","error","objective complete","gate open","bridge repair","crystal activation","beacon","dragon rescue"};
            for(int k=0;k<keys.Length;k++) {
                int count=k==7?44100:11025; var data=new float[count];
                for(int i=0;i<count;i++) { float t=i/44100f; float envelope=Mathf.Sin(Mathf.PI*i/count); float pitch=(k==1?160:360+k*65)*(i<count/2?1:1.25f); data[i]=Mathf.Sin(t*pitch*Mathf.PI*2)*envelope*.38f; }
                var clip=AudioClip.Create(keys[k],count,1,44100,false); clip.SetData(data,0); clips[keys[k]]=clip;
            }
        }
        public void Play(string id) { if(source && clips.TryGetValue(id,out var clip)) source.PlayOneShot(clip); }
    }
}
