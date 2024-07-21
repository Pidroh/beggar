//using UnityEngine.U2D;

using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.View.CursorManager;

namespace HeartUnity.View
{
    public class SelectableManager
    {
        public SelectableGroup activeGroup;
        public SelectableUnit selectedNonMouseElement;
        public CursorManager cursorManager = new CursorManager();
        public InputManager inputManager;

        public void Enter(SelectableGroup group, UIUnit unit = null)
        {
            activeGroup = group;
            unit = unit == null ? group.firstElement : unit;
            if (unit == null)
            {
                foreach (var su in activeGroup.selectables)
                {
                    if (su.IsActive)
                    {
                        selectedNonMouseElement = su;
                        cursorManager.cursorView.gameObject.SetActive(selectedNonMouseElement != null && inputManager.latestInputDevice != InputManager.InputDevice.MOUSE);
                        break;
                    }
                }
            }
            else
            {
                foreach (var su in activeGroup.selectables)
                {
                    if (su.IsActive && su.uiUnit == unit)
                    {
                        selectedNonMouseElement = su;
                        cursorManager.cursorView.gameObject.SetActive(selectedNonMouseElement != null && inputManager.latestInputDevice != InputManager.InputDevice.MOUSE);
                        break;
                    }
                }
            }

        }

        public void ManualUpdate()
        {
            if (activeGroup == null) return;
            // -----------------------------------------------------------------------------
            // IF MOUSE IS DOMINANT, TRY TO MAKE MOUSE HOVERED THING BE THE CURRENT SELECTED ELEMENT
            // -----------------------------------------------------------------------------
            if (inputManager.latestInputDevice == InputManager.InputDevice.MOUSE)
            {
                foreach (var su in activeGroup.selectables)
                {
                    if (su.uiUnit.HoveredWhileVisible)
                    {
                        this.selectedNonMouseElement = su;
                    }
                }
            }
            // -----------------------------------------------------------------------------
            // CHECK INPUT FOR MOVING THE CURSOR
            // -----------------------------------------------------------------------------
            {

                Direction d = Direction.ANY;
                if (inputManager.IsButtonDownOrRepeat(DefaultButtons.LEFT))
                {
                    d = Direction.WEST;

                }
                if (inputManager.IsButtonDownOrRepeat(DefaultButtons.UP))
                {
                    d = Direction.NORTH;

                }
                if (inputManager.IsButtonDownOrRepeat(DefaultButtons.RIGHT))
                {
                    d = Direction.EAST;
                }
                if (inputManager.IsButtonDownOrRepeat(DefaultButtons.DOWN))
                {
                    d = Direction.SOUTH;

                }
                MoveInDirection(d);
            }
            // -----------------------------------------------------------------------------
            // SELECTED INACTIVE HANDLING
            // -----------------------------------------------------------------------------
            if (selectedNonMouseElement != null && !selectedNonMouseElement.IsActive && activeGroup.FallbackElement != null)
            {
                selectedNonMouseElement = activeGroup.FallbackElement;
            }
                
            for (int i = 0; i < 4; i++)
            {
                if (selectedNonMouseElement != null && !selectedNonMouseElement.IsActive)
                {
                    switch (i)
                    {
                        case 0:
                            MoveInDirection(Direction.SOUTH);
                            break;
                        case 1:
                            MoveInDirection(Direction.NORTH);
                            break;
                        case 2:
                            MoveInDirection(Direction.WEST);
                            break;
                        case 3:
                            MoveInDirection(Direction.EAST);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    break;
                }
            }

            // -----------------------------------------------------------------------------
            // CHECK INPUT FOR CLICKING
            // -----------------------------------------------------------------------------
            if (inputManager.IsButtonDown(DefaultButtons.CONFIRM) && selectedNonMouseElement != null)
            {
                selectedNonMouseElement.uiUnit.ForceClick();
            }
            // -----------------------------------------------------------------------------
            // UPDATE CURSOR POSITION BASED ON CURRENT SELECTED
            // -----------------------------------------------------------------------------

            if (selectedNonMouseElement != null)
            {
                Vector2 selfSizeMul = Vector2.zero;
                Vector2 cursorSizeMul = Vector2.zero;
                Vector2 disMul = Vector2.zero;
                CursorPositionBehavior cursorBehavior = selectedNonMouseElement.cursorBehavior;
                Vector3 referencePositionForCursor = cursorManager.PositionToLocalPosition(selectedNonMouseElement.rectTransform.position);
                // transform from ANY to an appropriate behavior
                if (cursorBehavior == CursorPositionBehavior.ANY)
                {
                    //Vector3 referencePositionForCursor = selectedNonMouseElement.rectTransform.position;
                    //Vector3 referencePositionForCursor = cursorManager.cursorView.transform.parent.InverseTransformVector(selectedNonMouseElement.rectTransform.TransformVector(selectedNonMouseElement.rectTransform.localPosition));

                    if (Mathf.Abs(referencePositionForCursor.x) > Mathf.Abs(referencePositionForCursor.y))
                    {
                        if (referencePositionForCursor.x > 0)
                            cursorBehavior = CursorPositionBehavior.LEFT;
                        else
                            cursorBehavior = CursorPositionBehavior.RIGHT;
                    }
                    else
                    {
                        if (referencePositionForCursor.y > 0)
                            cursorBehavior = CursorPositionBehavior.DOWN;
                        else
                            cursorBehavior = CursorPositionBehavior.UP;
                    }
                }
                switch (cursorBehavior)
                {
                    case CursorPositionBehavior.LEFT:
                        selfSizeMul.x = -1;
                        cursorSizeMul.x = -1;
                        disMul.x = -1;
                        break;
                    case CursorPositionBehavior.RIGHT:
                        selfSizeMul.x = 1;
                        cursorSizeMul.x = 1;
                        disMul.x = 1;
                        break;
                    case CursorPositionBehavior.UP:
                        selfSizeMul.y = 1;
                        cursorSizeMul.y = 1;
                        disMul.y = 1;
                        break;
                    case CursorPositionBehavior.DOWN:
                        selfSizeMul.y = -1;
                        cursorSizeMul.y = -1;
                        disMul.y = -1;
                        break;
                    default:
                        break;
                }
                var selfOffset = Vector2.Scale(selfSizeMul, selectedNonMouseElement.rectTransform.sizeDelta) * 0.5f;
                var cursorOffset = Vector2.Scale(cursorSizeMul, cursorManager.GetCursorSizeDelta()) * 0.5f;
                var cursorDisOffset = disMul * cursorManager.distance;
                var offset = selfOffset + cursorOffset + cursorDisOffset;
                cursorManager.TargetCursorLocalPosition = referencePositionForCursor + new Vector3(offset.x, offset.y);
                cursorManager.SetCurrentBehavior(cursorBehavior);
                cursorManager.cursorView.gameObject.SetActive(inputManager.latestInputDevice != InputManager.InputDevice.MOUSE & inputManager.InputEnabled);
            }
            cursorManager.Update();
        }

        public void Select(UIUnit uu) 
        {
            foreach (var item in activeGroup.selectables)
            {
                if (item.uiUnit == uu) selectedNonMouseElement = item;
            }
        }

        private void MoveInDirection(Direction d)
        {
            if (selectedNonMouseElement != null)
            {
                if (selectedNonMouseElement.preferredElementInDirection.TryGetValue(d, out var prefer))
                {
                    if (prefer.Active) {
                        foreach (var su in activeGroup.selectables)
                        {
                            if (su.uiUnit == prefer) {
                                selectedNonMouseElement = su;
                                return;
                            }
                        }
                        
                    }
                }
            }
            Vector2Int weight = Vector2Int.zero;
            bool strongX = false;
            bool strongY = false;
            switch (d)
            {
                case Direction.ANY:
                    break;
                case Direction.NORTH:
                    weight = new Vector2Int(1, 1);
                    strongY = true;
                    break;
                case Direction.SOUTH:
                    weight = new Vector2Int(1, -1);
                    strongY = true;
                    break;
                case Direction.EAST:
                    weight = new Vector2Int(1, 1);
                    strongX = true;
                    break;
                case Direction.WEST:
                    weight = new Vector2Int(-1, 1);
                    strongX = true;
                    break;
                default:
                    break;
            }

            if (weight != Vector2Int.zero)
            {
                if (this.activeGroup.mode == SelectableGroup.SelectableGroupMode.MISS_INTOLERANT)
                {
                    if (strongX) weight.x *= 10000;
                    if (strongY) weight.y *= 10000;
                }
                else
                {
                    if (strongX) weight.y *= 2;
                    if (strongY) weight.x *= 2;
                }
                var selectedUnit = MoveCursor(weight, strongX, strongY, d);
                if (selectedUnit != null)
                {
                    selectedNonMouseElement = selectedUnit;
                }

            }
            SelectableUnit MoveCursor(Vector2Int moveCursor, bool xStrong, bool yStrong, Direction d)
            {
                var weight = float.MaxValue;
                SelectableUnit selectedElement = null;
                foreach (var selec in activeGroup.selectables)
                {
                    if (!selec.IsActive) continue;
                    if (selectedNonMouseElement.IsBlacklisted(d, selec.uiUnit)) continue;

                    if (selec != this.selectedNonMouseElement)
                    {
                        var dist = selec.rectTransform.position - selectedNonMouseElement.rectTransform.position;
                        var xComp = dist.x * moveCursor.x;
                        var yComp = dist.y * moveCursor.y;
                        if (xStrong && xComp == 0)
                        {
                            continue;
                        }
                        if (yStrong && yComp == 0)
                        {
                            continue;
                        }
                        if (activeGroup.mode == SelectableGroup.SelectableGroupMode.MISS_TOLERANT)
                        {
                            if (xStrong && xComp <= 0)
                            {
                                continue;
                            }
                            if (yStrong && yComp <= 0)
                            {
                                continue;
                            }
                        }
                        if (xStrong && yComp < 0) yComp *= -1;
                        if (yStrong && xComp < 0) xComp *= -1;
                        var thisWeight = xComp + yComp;

                        if (thisWeight > 0 && thisWeight < weight)
                        {
                            selectedElement = selec;
                            weight = thisWeight;
                        }
                    }
                }
                return selectedElement;
            }
        }

        public SelectableUnit GetSelected()
        {
            // if anyone on the group is hovered, return itself
            if (inputManager.latestInputDevice == InputManager.InputDevice.MOUSE)
            {
                foreach (var su in activeGroup.selectables)
                {
                    if (!su.IsActive) continue;
                    if (su.uiUnit.HoveredWhileVisible) return su;
                }
                return null;
            }
            else
            {
                return selectedNonMouseElement;
            }

        }

        public bool IsSelected(UIUnit item)
        {
            if (inputManager.latestInputDevice == InputManager.InputDevice.MOUSE) return item.HoveredWhileVisible;
            else
            {
                return selectedNonMouseElement.uiUnit == item;
            }
            /*
             - If the tested element is NOT inside the selectable group, return mouse highlight
             - if it is inside the selectable group, return true if mouse highlight. If not highlight, return true if it is keyboard selected
             */
            if (IsInside(item))
            {

                if (item.HoveredWhileVisible) return true;
                else
                {
                    // if anyone on the group is hovered, return false
                    foreach (var su in activeGroup.selectables)
                    {
                        if (su.uiUnit.HoveredWhileVisible && su.uiUnit != item) return false;
                    }
                    // return true if no one relevant is hovered AND the item is selected
                    return selectedNonMouseElement.uiUnit == item;
                }
            }
            else
            {
                return item.HoveredWhileVisible;
            }

        }

        private bool IsInside(UIUnit item)
        {
            if (activeGroup == null) return false;
            foreach (var su in activeGroup.selectables)
            {
                if (item == su.uiUnit)
                {
                    return true;
                }
            }
            return false;
        }

        public void ChangeCursorBehavior(UIUnit unit, CursorPositionBehavior cursorB)
        {
            foreach (var su in activeGroup.selectables)
            {
                if (su.uiUnit == unit)
                {
                    su.cursorBehavior = cursorB;
                }
            }
        }

        public void Enforce(SelectableSnapshot selectableSnapshot)
        {
            activeGroup = selectableSnapshot.selectableGroup;
            selectedNonMouseElement = selectableSnapshot.selectedUnit;
        }

        public SelectableSnapshot SaveSnapshot()
        {
            return new SelectableSnapshot(activeGroup, selectedNonMouseElement);
        }

        public struct SelectableSnapshot
        {
            public SelectableGroup selectableGroup;
            public SelectableUnit selectedUnit;

            public SelectableSnapshot(SelectableGroup selectableGroup, SelectableUnit selectedUnit)
            {
                this.selectableGroup = selectableGroup;
                this.selectedUnit = selectedUnit;
            }
        }
    }

    public class SelectableGroup
    {
        public List<SelectableUnit> selectables = new();
        public SelectableGroupMode mode = SelectableGroupMode.MISS_INTOLERANT;
        public UIUnit firstElement = null;


        public SelectableGroup(SelectableGroupMode mode = SelectableGroupMode.MISS_INTOLERANT)
        {
            this.mode = mode;
        }

        public SelectableUnit FallbackElement { get; set; }

        public SelectableUnit Add(UIUnit item, CursorPositionBehavior behavior = CursorPositionBehavior.ANY)
        {
            SelectableUnit su = new SelectableUnit(item);
            su.cursorBehavior = behavior;
            selectables.Add(su);
            return su;
        }

        public void Deactivate(UIUnit unit)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit != unit) continue;
                su.SetActive(false);
                return;
            }
        }

        public void AddOrActivate(UIUnit unit, CursorPositionBehavior? behavior)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit != unit) continue;
                su.SetActive(true);
                return;
            }
            if (behavior.HasValue)
                Add(unit, behavior.Value);
            else
                Add(unit);
        }

        public void ChangeCursorBehavior(UIUnit unit, CursorPositionBehavior cursorB)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit == unit) su.cursorBehavior = cursorB;
            }
        }

        public void ClearPreferredElementInDirection(UIUnit unit, Direction d)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit == unit) su.ClearPreferred(d);
            }
        }



        public void SetFallbackElement(UIUnit unit)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit == unit) FallbackElement = su;
            }
        }

        public void SetPreferredElementInDirection(UIUnit unit, Direction d, UIUnit preferred)
        {
            foreach (var su in selectables)
            {
                if (su.uiUnit == unit)
                {
                    su.SetPreferred(d, preferred);
                }
            }
        }

        public enum SelectableGroupMode
        {
            MISS_INTOLERANT,
            MISS_TOLERANT
        }


    }

    public class SelectableUnit
    {
        public UIUnit uiUnit;
        public RectTransform rectTransform;
        public CursorPositionBehavior cursorBehavior = CursorPositionBehavior.ANY;
        private bool _active = true;
        public List<SelectableTransitionBlacklist> transitionBlacklist = new();
        public Dictionary<Direction, UIUnit> preferredElementInDirection = new();

        public void SetActive(bool b) => _active = b;

        public void Blacklist(UIUnit unit, Direction d)
        {
            transitionBlacklist.Add(new SelectableTransitionBlacklist(unit, d));
        }

        public bool IsBlacklisted(Direction d, UIUnit uiUnit)
        {
            foreach (var bl in transitionBlacklist)
            {
                if (bl.direction == d && bl.transitionTarget == uiUnit)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetPreferred(Direction d, UIUnit preferred)
        {
            preferredElementInDirection[d] = preferred;
        }

        public void ClearPreferred(Direction d)
        {
            preferredElementInDirection.Remove(d);
        }

        public SelectableUnit(UIUnit uiUnit)
        {
            this.uiUnit = uiUnit;
            rectTransform = uiUnit.GetComponent<RectTransform>();
        }

        public bool IsActive => _active && uiUnit.Active;
    }

    public class SelectableTransitionBlacklist
    {
        public UIUnit transitionTarget;
        public Direction direction;

        public SelectableTransitionBlacklist(UIUnit transitionTarget, Direction direction)
        {
            this.transitionTarget = transitionTarget;
            this.direction = direction;
        }
    }
    public enum Direction
    {
        ANY, NORTH, SOUTH, EAST, WEST
    }
}