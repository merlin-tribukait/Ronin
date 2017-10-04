using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;

namespace Ronin.Data.Structures
{
    public class Locatable : INotifyPropertyChanged
    {
        private int x, y, z;

        public long MoveToStartStamp = 0;

        public int destX, destY, destZ;

        private bool isMoving = false;
        private bool isFollowing = false;
        public Locatable UnitToFollow;
        private bool _isRunning = false;
        public int RunningSpeed = 160;
        public int WalkingSpeed = 50;
        public double MovementSpeedMultiplier = 1;
        private long updateStamp = 0;

        public int MovingSpeed
        {
            get { return (int)((IsRunning ? this.RunningSpeed : WalkingSpeed ) * this.MovementSpeedMultiplier); }
        }

        public Locatable()
        {
        }

        public Locatable(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double RangeTo(Locatable unit)
        {
            UpdateLocation();
            unit.UpdateLocation();
            double result = Math.Sqrt(Math.Pow((unit.x - this.x), 2) + Math.Pow((unit.y - this.y), 2) + Math.Pow((unit.z - this.z), 2));
            return result;
        }

        public Locatable Loc
        {
            get
            {
                _forceUpdate = true;
                UpdateLocation();
                return new Locatable(x,y,z);
            }
        }

        private bool _forceUpdate = false;

        public string CoordsUI
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                sb.Append(this.x);
                sb.Append(", ");
                sb.Append(this.y);
                sb.Append(", ");
                sb.Append(this.z);

                sb.Append(")");
                return sb.ToString();
            }
        }

        private void UpdateLocation()
        {
            //TODO: DONT FORGET ME
            if (Environment.TickCount - updateStamp < 10 && !_forceUpdate)
            {
                return;
            }

            updateStamp = Environment.TickCount;
            _forceUpdate = false;

            if (this.isMoving)
            {
                double distance = Math.Sqrt(Math.Pow((destX - this.x), 2) + Math.Pow((destY - this.y), 2) + Math.Pow((destZ - this.z), 2));
                if (distance < 20)
                {
                    this.isMoving = false;
                    return;
                }

                int differenceInX = this.destX - this.x;
                int differenceInY = this.destY - this.y;
                int differenceInZ = this.destZ - this.z;


                double wholeTripTime = (distance / this.MovingSpeed) * 1000;

                // If elapsed time is bigger than the whole trip time, set trip time.
                long elapsedTime = (Math.Abs(Environment.TickCount - this.MoveToStartStamp)) > wholeTripTime ? (long)wholeTripTime : Math.Abs(Environment.TickCount - this.MoveToStartStamp);
                this.x += (int)(differenceInX * (elapsedTime / wholeTripTime));
                this.y += (int)(differenceInY * (elapsedTime / wholeTripTime));
                this.z += (int)(differenceInZ * (elapsedTime / wholeTripTime));

                this.MoveToStartStamp = Environment.TickCount;

                distance = Math.Sqrt(Math.Pow((destX - this.x), 2) + Math.Pow((destY - this.y), 2) + Math.Pow((destZ - this.z), 2));

                if (distance < 20)
                {
                    this.isMoving = false;
                    return;
                }
            }
            else if (isFollowing && UnitToFollow != null)
            {
                UnitToFollow.UpdateLocation();
                destX = UnitToFollow.x;
                destY = UnitToFollow.y;
                destZ = UnitToFollow.z;

                double distance = Math.Sqrt(Math.Pow((destX - this.x), 2) + Math.Pow((destY - this.y), 2) + Math.Pow((destZ - this.z), 2));

                if (distance < 20)
                {
                    this.isFollowing = false;
                    return;
                }

                int differenceInX = this.destX - this.x;
                int differenceInY = this.destY - this.y;
                int differenceInZ = this.destZ - this.z;

                distance = Math.Sqrt(Math.Pow((destX - this.x), 2) + Math.Pow((destY - this.y), 2) + Math.Pow((destZ - this.z), 2));
                double wholeTripTime = (distance / this.MovingSpeed) * 1000;

                // If elapsed time is bigger than the whole trip time, set trip time.
                long elapsedTime = (Math.Abs(Environment.TickCount - this.MoveToStartStamp)) > wholeTripTime ? (long)wholeTripTime : Math.Abs(Environment.TickCount - this.MoveToStartStamp);
                this.x += (int)(differenceInX * (elapsedTime / wholeTripTime));
                this.y += (int)(differenceInY * (elapsedTime / wholeTripTime));
                this.z += (int)(differenceInZ * (elapsedTime / wholeTripTime));
                
                this.MoveToStartStamp = Environment.TickCount;

                distance = Math.Sqrt(Math.Pow((destX - this.x), 2) + Math.Pow((destY - this.y), 2) + Math.Pow((destZ - this.z), 2));

                if (distance < 20)
                {
                    this.isFollowing = false;
                    return;
                }
            }
        }

        public int X
        {
            get
            {
                UpdateLocation();
                return x;
            }
            set
            {
                x = value; 
                OnPropertyChanged();
            }
        }

        public int Y
        {
            get
            {
                UpdateLocation();
                return y;
            }
            set
            {
                y = value;
                OnPropertyChanged();
            }
        }

        public int Z
        {
            get
            {
                UpdateLocation();
                return z;
            }
            set
            {
                z = value;
                OnPropertyChanged();
            }
        }

        public bool IsMoving
        {
            get
            {
                UpdateLocation();
                return isMoving;
            }
            set
            {
                UpdateLocation();
                isMoving = value;
            }
        }

        public bool IsFollowing
        {
            get { return isFollowing; }
            set
            {
                UpdateLocation();
                isFollowing = value;
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                UpdateLocation();
                _isRunning = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
