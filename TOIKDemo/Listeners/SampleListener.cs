using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using TOIKDemo.Common;

public delegate void HandAppearDelegate();
public delegate void HandDisappearDelegate();

namespace TOIKDemo.Listeners
{
    public class SampleListener : Listener
    {
        private Object thisLock = new Object();
        private FrameState previousFrameState;
        private SwipeGesture previousGesture;

        public HandAppearDelegate OnOneHandAppear;
        public HandDisappearDelegate OnHandDisappear;



        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            previousFrameState = new FrameState();
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            Frame frame = controller.Frame();

            ControlHandEvents(frame.Hands.Count);
            GestureList list = frame.Gestures();
            if (frame.Gestures().Count > 0 && frame.Gestures().First().Type == Gesture.GestureType.TYPESWIPE)
            {
                SwipeGesture swipe = new SwipeGesture(frame.Gestures().First());
                var speed = swipe.Speed;
                if (previousGesture == null || DetermineDirection(swipe) != DetermineDirection(previousGesture))
                {
                    SafeWriteLine("Swipe Move detected. Direction: " + DetermineDirection(swipe) + ". Speed " + speed);
                    previousGesture = swipe;
                }
            }
            else
            {
                previousGesture = null;
            }
            SetPreviousFrameState(frame.Hands.Count);
        }

        private Direction DetermineDirection(SwipeGesture gesture)
        {
            var direction = gesture.Direction;
            if (gesture.Direction.x < 0 && Math.Abs(direction.x) > Math.Abs(direction.z))
                return Direction.West;
            if (gesture.Direction.x >= 0 && Math.Abs(direction.x) > Math.Abs(direction.z))
                return Direction.East;
            if (gesture.Direction.z < 0 && Math.Abs(direction.z) > Math.Abs(direction.x))
                return Direction.North;
            if (gesture.Direction.z >= 0 && Math.Abs(direction.z) > Math.Abs(direction.x))
                return Direction.South;
            return Direction.South;
        }

        private void SetPreviousFrameState(int handsCount)
        {
            previousFrameState.HandsCount = handsCount;
        }

        public void RegisterOnOneHandAppearListener(HandAppearDelegate listener)
        {
            OnOneHandAppear += listener;
        }

        private void ControlHandEvents(int handsCount)
        {
            if (previousFrameState.HandsCount == 0 && handsCount > previousFrameState.HandsCount && handsCount == 1)
            {
                //SafeWriteLine("One hand appeared");
                if (OnOneHandAppear != null)
                {
                    OnOneHandAppear();
                }
            }
            else if (previousFrameState.HandsCount == 1 && handsCount > previousFrameState.HandsCount)
            {
                //SafeWriteLine("Now second hand appeared");
            }
            else if (previousFrameState.HandsCount == 2 && handsCount < previousFrameState.HandsCount && handsCount == 1)
            {
                //SafeWriteLine("One hand has disappeared");
            }
            else if (handsCount < previousFrameState.HandsCount && handsCount == 0)
            {
                //SafeWriteLine("All hands have disappeared");
                if (OnHandDisappear != null)
                {
                    OnHandDisappear();
                }
            }
        }
    }

    enum Direction
    {
        North,South,East,West,Up,Down
    }
}
