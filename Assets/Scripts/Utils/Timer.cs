using UnityEngine;

namespace Wyru
{
    public delegate void OnTimerEnd();

    public class Timer : MonoBehaviour
    {
        [SerializeField] float currentTime;
        [SerializeField] float time;
        [SerializeField] bool running = false;
        [SerializeField] bool ended;

        public event OnTimerEnd onTimerEnd;


        public void SetTimer(float time)
        {
            this.time = time;
        }

        public void SetTimer(float time, OnTimerEnd OnTimerEnd)
        {
            this.time = time;
            this.onTimerEnd = OnTimerEnd;
        }

        public float GetProgress()
        {
            return Mathf.Clamp01(currentTime / time);
        }

        private void Update()
        {
            if (running && !ended)
            {
                currentTime += Time.deltaTime;
                if (GetProgress() == 1)
                {
                    onTimerEnd?.Invoke();
                    ended = true;
                }
            }
        }

        public void StartTimer()
        {
            currentTime = 0;
            running = true;
        }

        public void StartTimer(bool restartIfRunning)
        {
            if (running)
            {
                if (restartIfRunning)
                {
                    currentTime = 0;
                    running = true;
                    ended = false;
                }
            }
            else
            {
                currentTime = 0;
                running = true;
                ended = false;
            }

        }

        public void StopTimer()
        {
            currentTime = 0;
            running = false;
            ended = false;
        }

    }
}