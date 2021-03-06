﻿using GameHelper.Camera.Cameras;
using GameHelper.Objects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameHelper.Camera
{
    public class CameraManager
    {
        SortedList<int, BaseCamera> Cameras = new SortedList<int, BaseCamera>();
        public BaseCamera currentCamera = null;
        SortedList<int, SortedList<int, ViewProfile>> Views = new SortedList<int, SortedList<int, ViewProfile>>();
        

        #region Initialization
        public CameraManager()
        {
        }

        public void NextCamera()
        {
            if (Cameras.Count == 0)
                return;

            int index = Cameras.IndexOfKey(currentCamera.id);
            index++;
            if (index >= Cameras.Count)
                index = 0;
            currentCamera= Cameras.Values[index];            
        }

        public void AddCamera(int camId, BaseCamera newCam)
        {
            if (GetCamera(camId) != null) return;

            newCam.id = camId;
            Cameras.Add(camId, newCam);

            /// if this is the first camera, make it the default
            if (Cameras.Count == 1)
                SetCurrentCamera(camId);
        }

        public void AddProfile(ViewProfile vp)
        {
            if (!Views.ContainsKey(vp.CameraId))
                Views.Add(vp.CameraId, new SortedList<int, ViewProfile>());
            SortedList<int, ViewProfile> camViews = Views[vp.CameraId];
            if (camViews.ContainsKey(vp.assetAlias))
                return;
            camViews.Add(vp.assetAlias, vp);

        }

        public void SetGobjectList(int camId, List<Entity> gobs)
        {
            BaseCamera cam = GetCamera(camId);
            if (cam == null) return;

            cam.SetGobjectList(gobs);

            SortedList<int, ViewProfile> camViews = new SortedList<int, ViewProfile>();
            try
            {
                if (Views.ContainsKey(camId))
                {
                    foreach (Entity gob in gobs)
                    {
                        if (gob == null)
                            continue;
                        int assettype = gob.aType.Id;
                        if (Views[camId].ContainsKey(assettype))
                            camViews.Add(assettype, Views[camId][assettype]);
                    }
                }
            }
            catch (Exception E)
            {
                Debug.WriteLine(E.StackTrace);
            }
            finally
            {
                cam.SetProfiles(camViews);
            }
            
        }
        #endregion

        #region Current Camera
        
        public void SetCurrentCamera(int camId)
        {
            BaseCamera cam = GetCamera(camId);
            if (cam == null) return;
            currentCamera = cam;
        }

        public void Update()
        {
            if (currentCamera == null) return;
            currentCamera.Update();
        }

        public Matrix ViewMatrix()
        {
            if (currentCamera == null) return Matrix.Identity;
            return currentCamera.GetViewMatrix();
        }

        public Matrix ProjectionMatrix()
        {
            if (currentCamera == null) return Matrix.Identity;
            return currentCamera.GetProjectionMatrix();
        }
        #endregion

        #region Utility
        private BaseCamera GetCamera(int camId)
        {
            if (!Cameras.ContainsKey(camId))
                return null;
            return Cameras[camId];
        }
        #endregion

        public void IncreaseMovementSpeed()
        {
            currentCamera.IncreaseMovementSpeed();
        }

        public void AdjustTargetOrientationTo(float p, float y)
        {
            currentCamera.AdjustTargetOrientationTo(p,y);
        }
        public void AdjustTargetOrientationBy(float p, float y)
        {
            currentCamera.AdjustTargetOrientationBy(p, y);
        }

        public void DecreaseMovementSpeed()
        {
            currentCamera.DecreaseMovementSpeed();
        }
        public void ZoomIn()
        {
            currentCamera.ZoomIn();
        }
        public void ZoomOut()
        {
            currentCamera.ZoomOut();

        }
        public void MoveUp()
        {
            currentCamera.MoveUp();
        }

        public void MoveDown()
        {
            currentCamera.MoveDown();
        }

        public void MoveForward()
        {
            currentCamera.MoveForward();
        }

        public void MoveBackward()
        {
            currentCamera.MoveBackward();
        }

        public void MoveLeft()
        {
            currentCamera.MoveLeft();
        }

        public void MoveRight()
        {
            currentCamera.MoveRight();
        }


        public void SetWorldMatrix(Matrix matrix)
        {
        }
    }
}
