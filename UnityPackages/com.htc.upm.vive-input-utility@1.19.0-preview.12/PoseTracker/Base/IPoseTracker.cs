﻿//========= Copyright 2016-2024, HTC Corporation. All rights reserved. ===========

namespace HTC.UnityPlugin.PoseTracker
{
    public interface IPoseTracker
    {
        void AddModifier(IPoseModifier obj);
        bool RemoveModifier(IPoseModifier obj);
    }
}