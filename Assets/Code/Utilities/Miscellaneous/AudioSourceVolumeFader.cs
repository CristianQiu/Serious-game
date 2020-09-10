﻿using UnityEngine;

/// <summary>
/// A class used to fade the volume of an AudioSource
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourceVolumeFader : MonoBehaviour
{
    #region Delegates

    // TODO: This delegates should be in a manager holding all the UI elements that could be faded
    public delegate void FinishedFadingInEventHandler(object source, System.EventArgs args);

    public event FinishedFadingInEventHandler FinishedFadingIn;

    public delegate void FinishedFadingOutEventHandler(object source, System.EventArgs args);

    public event FinishedFadingOutEventHandler FinishedFadingOut;

    #endregion

    #region Public Attributes

    [Header("Values")]
    [Range(0.0f, 1.0f)]
    public float fadedOutValue = 0.0f;

    [Range(0.0f, 1.0f)]
    public float fadedInValue = 1.0f;

    [Header("Time")]
    public float timeToFade = 0.75f;

    [Header("Type")]
    public InterpolationFunction fadeType = InterpolationFunction.Linear;

    [Header("Starting state")]
    public FadeState startState = FadeState.FadedIn;

    #endregion

    #region Private Attributes

    private AudioSource audioSource = null;
    private FadeState currState = FadeState.Invalid;
    private float currValue = 0.0f;
    private float timer = 0.0f;

    #endregion

    #region Properties

    public float CurrValue { get { return currValue; } }
    public bool FadedOut { get { return currState == FadeState.FadedOut; } }
    public bool FadingIn { get { return currState == FadeState.FadingIn; } }
    public bool FadedIn { get { return currState == FadeState.FadedIn; } }
    public bool FadingOut { get { return currState == FadeState.FadingOut; } }

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SwitchState(startState, 0.0f, true);
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        UpdateState(dt);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialization
    /// </summary>
    private void Init()
    {
    }

    #endregion

    #region State Methods

    /// <summary>
    /// Switch the current state for a new one
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="forcedStartTime"></param>
    /// <param name="force"></param>
    private void SwitchState(FadeState newState, float forcedStartTime, bool force)
    {
        if (currState == newState && !force)
            return;

        switch (currState)
        {
            case FadeState.FadedOut:
                ExitFadedOut();
                break;

            case FadeState.FadingIn:
                ExitFadingIn();
                break;

            case FadeState.FadedIn:
                ExitFadedIn();
                break;

            case FadeState.FadingOut:
                ExitFadingOut();
                break;

            default:
                Debug.Log("Invalid FadeState : " + currState);
                break;
        }

        switch (newState)
        {
            case FadeState.FadedOut:
                EnterFadedOut();
                break;

            case FadeState.FadingIn:
                EnterFadingIn(forcedStartTime);
                break;

            case FadeState.FadedIn:
                EnterFadedIn();
                break;

            case FadeState.FadingOut:
                EnterFadingOut(forcedStartTime);
                break;

            default:
                Debug.Log("Invalid FadeState : " + newState);
                break;
        }

        currState = newState;
    }

    /// <summary>
    /// Enter the faded out state
    /// </summary>
    private void EnterFadedOut()
    {
        timer = 0.0f;
        SetCurrVolume(fadedOutValue);
    }

    /// <summary>
    /// Enter the fading in state
    /// </summary>
    /// <param name="forcedStartTime"></param>
    private void EnterFadingIn(float forcedStartTime)
    {
        timer = forcedStartTime;
    }

    /// <summary>
    /// Enter the faded in state
    /// </summary>
    private void EnterFadedIn()
    {
        timer = 0.0f;
        SetCurrVolume(fadedInValue);
    }

    /// <summary>
    /// Enter the fading out state
    /// </summary>
    /// <param name="forcedStartTime"></param>
    private void EnterFadingOut(float forcedStartTime)
    {
        timer = forcedStartTime;
    }

    /// <summary>
    /// Exit the faded out state
    /// </summary>
    private void ExitFadedOut()
    {
        timer = 0.0f;
        SetCurrVolume(fadedOutValue);
    }

    /// <summary>
    /// Exit the fading in state
    /// </summary>
    private void ExitFadingIn()
    {
        timer = 0.0f;
    }

    /// <summary>
    /// Exit the faded in state
    /// </summary>
    private void ExitFadedIn()
    {
        timer = 0.0f;
        SetCurrVolume(fadedInValue);
    }

    /// <summary>
    /// Exit the fading out state
    /// </summary>
    private void ExitFadingOut()
    {
        timer = 0.0f;
    }

    /// <summary>
    /// Update the state
    /// </summary>
    /// <param name="dt"></param>
    private void UpdateState(float dt)
    {
        timer += dt;

        switch (currState)
        {
            case FadeState.FadedOut:
                break;

            case FadeState.FadingIn:
                UpdateFadingIn(dt);
                break;

            case FadeState.FadedIn:
                break;

            case FadeState.FadingOut:
                UpdateFadingOut(dt);
                break;

            default:
                Debug.Log("Invalid FadeState : " + currState);
                break;
        }
    }

    /// <summary>
    /// Update the fade in state
    /// </summary>
    /// <param name="dt"></param>
    private void UpdateFadingIn(float dt)
    {
        float t = timer / timeToFade;

        t = Mathf.Clamp01(t);
        float s = CustomInterpolation.Interpolate(t, fadeType);
        s = Mathf.Lerp(fadedOutValue, fadedInValue, s);

        SetCurrVolume(s);

        if (t >= 1.0f)
        {
            OnFinishedFadingIn();
            SwitchState(FadeState.FadedIn, 0.0f, false);
        }
    }

    /// <summary>
    /// Update the fade out state
    /// </summary>
    /// <param name="dt"></param>
    private void UpdateFadingOut(float dt)
    {
        float t = timer / timeToFade;

        t = Mathf.Clamp01(t);
        t = 1.0f - t;
        float s = CustomInterpolation.Interpolate(t, fadeType);
        s = Mathf.Lerp(fadedOutValue, fadedInValue, s);

        SetCurrVolume(s);

        if (t <= 0.0f)
        {
            OnFinishedFadingOut();
            SwitchState(FadeState.FadedOut, 0.0f, false);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Set the current alpha value
    /// </summary>
    /// <param name="newValue"></param>
    private void SetCurrVolume(float newValue)
    {
        currValue = newValue;
        audioSource.volume = newValue;
    }

    /// <summary>
    /// Instantly fades out
    /// </summary>
    private void InstantFadeOut()
    {
        SetCurrVolume(fadedOutValue);
        SwitchState(FadeState.FadedOut, 0.0f, true);

        OnFinishedFadingOut();
    }

    /// <summary>
    /// Instantly fades in
    /// </summary>
    private void InstantFadeIn()
    {
        SetCurrVolume(fadedInValue);
        SwitchState(FadeState.FadedIn, 0.0f, true);

        OnFinishedFadingIn();
    }

    /// <summary>
    /// Start the fading process
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="forceInstant"></param>
    public void StartFade(FadeState newState, bool forceInstant)
    {
        bool dontAllow = (newState != FadeState.FadingIn) && (newState != FadeState.FadingOut) ||
                         (newState == FadeState.FadingIn && FadedIn) ||
                         (newState == FadeState.FadingOut && FadedOut) ||
                         (newState == FadeState.FadingIn && FadingIn) ||
                         (newState == FadeState.FadingOut && FadingOut);

        if (dontAllow)
            return;

        // handle the instant face
        if (forceInstant)
        {
            if (newState == FadeState.FadingOut)
                InstantFadeOut();
            else
                InstantFadeIn();

            return;
        }

        // if trying to reverse the fade while already fading we are going to set the timer to
        // reverse the direction smoothly
        float timeLeftToFinish = 0.0f;

        if (FadingIn || FadingOut)
            timeLeftToFinish = timeToFade - timer;

        SwitchState(newState, timeLeftToFinish, false);
    }

    #endregion

    #region Callbacks

    /// <summary>
    /// Called when the fading in finishes
    /// </summary>
    protected virtual void OnFinishedFadingIn()
    {
        if (FinishedFadingIn != null)
            FinishedFadingIn(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Called when the fading out finishes
    /// </summary>
    protected virtual void OnFinishedFadingOut()
    {
        if (FinishedFadingOut != null)
            FinishedFadingOut(this, System.EventArgs.Empty);
    }

    #endregion
}