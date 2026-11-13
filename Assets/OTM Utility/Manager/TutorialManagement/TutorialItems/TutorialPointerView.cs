using com.otm.ui;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
    public class TutorialPointerView : BaseView
    {
        #region private variables

        [SerializeField] TutorialPointer tutorialPointer;
        private List<TutorialPointer> pointers = new List<TutorialPointer>();

        #endregion
        #region overrided methods

        protected override void OnViewHideDone()
        {
            base.OnViewHideDone();
            Destroy(gameObject);
        }

        public override void HideView()
        {
            for (int i = 0; i < pointers.Count; i++)
            {
                pointers[i].Hide();
            }
            base.HideView();
        }

        #endregion

        #region public methods

        public void ShowView(TutorialPointerConfig[] tutorialPointerConfigs)
        {
            base.ShowView();
            CreatePointer(tutorialPointerConfigs);
        }

        public void ShowView(TutorialPointerFollowConfig tutorialPointerFollowConfigs)
        {
            base.ShowView();

            TutorialPointer p = Instantiate(tutorialPointer, transform);
            p.Show(tutorialPointerFollowConfigs);
            pointers.Add(p);
        }

        public void ShowForEditor(TutorialPointerConfig[] tutorialPointerConfigs)
        {
            ShowEditor();
            pointers = GetComponentsInChildren<TutorialPointer>().ToList();
            if (pointers.Count != tutorialPointerConfigs.Length)
            {
                CreatePointer(tutorialPointerConfigs);
                return;
            }
            for (int i = 0; i < pointers.Count; i++)
            {
                pointers[i].Show(tutorialPointerConfigs[i]);
            }
        }

        public void HideForEditor()
        {
            for (int i = 0; i < pointers.Count; i++)
            {
                DestroyImmediate(pointers[i].gameObject);
            }
            pointers.Clear();
            HideEditor();
        }

        #endregion

        #region private methods

        private void CreatePointer(TutorialPointerConfig[] tutorialUIMaskConfigs)
        {
            for (int i = 0; i < tutorialUIMaskConfigs.Length; i++)
            {
                TutorialPointer p = Instantiate(tutorialPointer, transform);
                p.Show(tutorialUIMaskConfigs[i]);
                pointers.Add(p);
            }
        }

        #endregion
    }

    public class TutorialPointerConfig
    {
        public TextAnchor anchor;
        public Vector2 anchorPosition;
        public float rotation;
        public bool useAnimation = true;
    }

    public class TutorialPointerFollowConfig
    {
        public Vector3 offeset;
        public Transform target;
        public float rotation;
        public bool useAnimation = true;
    }
}
