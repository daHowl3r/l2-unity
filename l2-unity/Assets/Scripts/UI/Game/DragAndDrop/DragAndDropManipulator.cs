using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DragAndDropManipulator : PointerManipulator
{

    private Vector2 targetStartPosition { get; set; }

    private Vector3 pointerStartPosition { get; set; }

    private bool enabled { get; set; }

    private VisualElement root { get; }

    public DragAndDropManipulator(VisualElement target , VisualElement root)
    {
        this.target = target;
        this.root = root;
    }



    protected override void RegisterCallbacksOnTarget()
    {
        // Register the four callbacks on target.
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        // Un-register the four callbacks from target.
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    // This method stores the starting position of target and the pointer,
    // makes target capture the pointer, and denotes that a drag is now in progress.
    private void PointerDownHandler(PointerDownEvent evt)
    {
        targetStartPosition = target.transform.position;
        pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        var vector = new Vector2(target.worldBound.position.x , target.worldBound.position.y);
        DragIcon.Instance.NewPosition(vector , target.style.backgroundImage.value);
        DragIcon.Instance.BringToFront1();
        enabled = true;
    }

    // This method checks whether a drag is in progress and whether target has captured the pointer.
    // If both are true, calculates a new position for target within the bounds of the window.
    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            // Vector3 pointerDelta = evt.position - pointerStartPosition;
            Vector3 pointerDelta = evt.position;

            var position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
            DragIcon.Instance.NewPosition(position , target.style.backgroundImage.value);
            DragIcon.Instance.BringToFront1();
        }
    }

    // This method checks whether a drag is in progress and whether target has captured the pointer.
    // If both are true, makes target release the pointer.
    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    // This method checks whether a drag is in progress. If true, queries the root
    // of the visual tree to find all slots, decides which slot is the closest one
    // that overlaps target, and sets the position of target so that it rests on top
    // of that slot. Sets the position of target back to its original position
    // if there is no overlapping slot.
    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            Debug.Log("PointerCaptureOutHandler DRAGGGGANDDROPPPP");
            VisualElement slotsContainer = root.Q<VisualElement>("AllPanel");

            UQueryBuilder<VisualElement> allSlots =
                slotsContainer.Query<VisualElement>(className: "imgbox0");

            UQueryBuilder<VisualElement> overlappingSlots =
                allSlots.Where(OverlapsTarget);

            VisualElement closestOverlappingSlot =
                FindClosestSlot(overlappingSlots);

            Vector3 closestPos = Vector3.zero;
            if (closestOverlappingSlot != null)
            {
                closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
            }
            target.transform.position =
                closestOverlappingSlot != null ?
                closestPos :
                targetStartPosition;

            enabled = false;
        }
    }

    private bool OverlapsTarget(VisualElement slot)
    {
        return target.worldBound.Overlaps(slot.worldBound);
    }

    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
    {
        List<VisualElement> slotsList = slots.ToList();
        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        foreach (VisualElement slot in slotsList)
        {
            Vector3 displacement =
                RootSpaceOfSlot(slot) - target.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    private Vector3 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return root.WorldToLocal(slotWorldSpace);
    }

}
