using UnityEngine;

namespace NantesGame.Gameplay
{
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [Range(0,1)] public float walkVolume = .4f;
        [Range(0,1)] public float runVolume = .7f;
        [Range(0,1)] public float slowVolume = .14f;
        [Range(0,1)] public float crouchVolume = .035f;
        PlayerMovement movement;
        PlayerInputHandler input;
        Rigidbody body;
        CapsuleCollider capsule;
        AudioSource speaker;
        AudioClip[] clips;
        Vector3 lastPosition;
        float travelled, standingHeight;
        int lastClip = -1;
        bool paused;

        void Start()
        {
            movement = GetComponent<PlayerMovement>();
            input = GetComponent<PlayerInputHandler>();
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            standingHeight = capsule.height;
            lastPosition = body.position;
            var child = new GameObject("Footsteps");
            child.transform.SetParent(transform, false);
            speaker = child.AddComponent<AudioSource>();
            speaker.playOnAwake = false; speaker.spatialBlend = 0;
            clips = Resources.LoadAll<AudioClip>("GameplayAudio/Footsteps");
        }

        void Update()
        {
            bool hold = Time.timeScale == 0;
            if(hold == paused) return;
            if(hold) speaker.Pause(); else speaker.UnPause();
            paused = hold;
        }

        void FixedUpdate()
        {
            Vector3 delta = body.position - lastPosition;
            lastPosition = body.position;
            float distance = new Vector2(delta.x, delta.z).magnitude;
            Vector3 foot = capsule.bounds.center;
            foot.y = capsule.bounds.min.y + .12f;
            bool onFloor = Physics.Raycast(foot, Vector3.down, .24f, ~0, QueryTriggerInteraction.Ignore);
            //Use distance travelled so pushing into a wall does not keep making steps.
            if(!movement.enabled || input.move.sqrMagnitude < .01f || !onFloor || distance < .005f || distance > .75f)
            {
                travelled = 0;
                return;
            }
            bool crouched = input.crouchHeld || capsule.height < standingHeight - .08f;
            bool running = !crouched && !input.walkHeld && input.sprintHeld;
            float stride = crouched ? 1.7f : running ? 2.45f : input.walkHeld ? 1.4f : 1.9f;
            travelled += distance;
            if(travelled < stride || clips.Length == 0) return;
            travelled %= stride;
            int clip = Random.Range(0, clips.Length);
            if(clip == lastClip) clip = (clip + 1) % clips.Length;
            lastClip = clip;
            speaker.clip = clips[clip];
            speaker.volume = crouched ? crouchVolume : running ? runVolume : input.walkHeld ? slowVolume : walkVolume;
            speaker.pitch = Random.Range(.96f, 1.04f) * (crouched ? .92f : running ? 1.04f : 1);
            speaker.Play();
        }
    }
}
