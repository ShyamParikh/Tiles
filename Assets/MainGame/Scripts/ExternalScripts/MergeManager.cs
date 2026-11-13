/*using System;
using System.Collections.Generic;
using UnityEngine;

public class MergeManager : BaseManager
{
    [Header("Merge Settings")]

    [Header("Up Merge")]
    [SerializeField] private float upDistance = 0.15f;
    [SerializeField] private float upDuration = 0.3f;
    [SerializeField] private LeanTweenType upEase;

    [Header("Smash Merge")]
    [SerializeField] private float smashDuration = 0.3f;
    [SerializeField] private LeanTweenType smashEase;

    [Header("Merge Effects")]
    [SerializeField] private ParticleSystem mergeParticles;

    protected override void SubscribeToEvents()
    {
        ItemSpotsManager.startMerge += OnStartMerge;
    }
    protected override void UnsubscribeFromEvents()
    {
        ItemSpotsManager.startMerge -= OnStartMerge;
    }

    private void OnStartMerge(List<Item> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            Vector3 targetPosition = items[i].transform.position + items[i].transform.up * upDistance;

            Action callback = null;

            if (i == 0)
            {
                callback = () => SmashItems(items);
            }

            LeanTween.move(items[i].gameObject, targetPosition, upDuration).setEase(upEase).setOnComplete(callback);
        }
    }

    private void SmashItems(List<Item> items)
    {
        items.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        float targetX = items[1].transform.position.x;

        LeanTween.moveX(items[0].gameObject, targetX, smashDuration).setEase(smashEase);
        LeanTween.moveX(items[2].gameObject, targetX, smashDuration).setEase(smashEase).setOnComplete(() => FinalizeMerge(items));
    }

    private void FinalizeMerge(List<Item> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            Destroy(items[i].gameObject);
        }

        ParticleSystem particles = Instantiate(mergeParticles, items[1].transform.position, Quaternion.identity, transform);
        particles.Play();
    }
}*/