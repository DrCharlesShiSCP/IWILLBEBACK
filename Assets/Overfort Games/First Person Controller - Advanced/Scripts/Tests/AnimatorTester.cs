using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{
    public class AnimatorTester : MonoBehaviour
    {
        public GameObject EmptyHands;
        [Header("Settings")]
        public int layer = 0;

        [Range(0, 1)]
        public float normalizedTransitionDuration = 0.25f;
        public float normalizedTimeOffset = 0;

        [Space(10)]
        public KeyCodeAnimation[] keyCodeAnimations;



        private Animator animator;
        // Update is called once per frame, allows you to play an animation by pressing the numeric keys on the keyboard

        private void Awake()
        {
            animator = EmptyHands.GetComponent<Animator>();
        }
        void Update()
        {
            foreach (var item in keyCodeAnimations)
            {
                if (Input.GetKeyDown(item.keyCode))
                {
                    if (item.overrideSettings)
                    {
                        animator.CrossFade(item.stateName, item.normalizedTransitionDuration, item.layer, item.normalizedTimeOffset);
                    }
                    else
                    {
                        animator.CrossFade(item.stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
                    }
                }
            }
        }

        [Serializable]
        public class KeyCodeAnimation
        {
            public string stateName;
            public KeyCode keyCode;
            [Header("Settings")]

            public bool overrideSettings = false;
            [Space(5)]

            public int layer = 0;
            public float normalizedTransitionDuration = 0.25f;
            public float normalizedTimeOffset = 0;
        }
    }

}
