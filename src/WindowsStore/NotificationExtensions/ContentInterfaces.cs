// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved

#if !WINRT_NOT_PRESENT
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
#endif

namespace NotificationsExtensions
{
    /// <summary>
    /// Base notification content interface to retrieve notification Xml as a string.
    /// </summary>
    interface INotificationContent
    {
        /// <summary>
        /// Retrieves the notification Xml content as a string.
        /// </summary>
        /// <returns>The notification Xml content as a string.</returns>
        string GetContent();

#if !WINRT_NOT_PRESENT
        /// <summary>
        /// Retrieves the notification Xml content as a WinRT Xml document.
        /// </summary>
        /// <returns>The notification Xml content as a WinRT Xml document.</returns>
        XmlDocument GetXml();
#endif
    }

    /// <summary>
    /// A type contained by the tile and toast notification content objects that
    /// represents a text field in the template.
    /// </summary>
    public interface INotificationContentText
    {
        /// <summary>
        /// The text value that will be shown in the text field.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The language of the text field.  This proprety overrides the language provided in the
        /// containing notification object.  The language should be specified using the
        /// abbreviated language code as defined by BCP 47.
        /// </summary>
        string Lang { get; set; }
    }

    /// <summary>
    /// A type contained by the tile and toast notification content objects that
    /// represents an image in a template.
    /// </summary>
    public interface INotificationContentImage
    {
        /// <summary>
        /// The location of the image.  Relative image paths use the BaseUri provided in the containing
        /// notification object.  If no BaseUri is provided, paths are relative to ms-appx:///.
        /// Only png and jpg images are supported.  Images must be 1024x1024 pixels or less, and smaller than
        /// 200 kB in size.
        /// </summary>
        string Src { get; set; }

        /// <summary>
        /// Alt text that describes the image.
        /// </summary>
        string Alt { get; set; }

        /// <summary>
        /// Controls if query strings that denote the client configuration of contrast, scale, and language setting should be appended to the Src
        /// If true, Windows will append query strings onto the Src
        /// If false, Windows will not append query strings onto the Src
        /// Query string details:
        /// Parameter: ms-contrast
        ///     Values: standard, black, white
        /// Parameter: ms-scale
        ///     Values: 80, 100, 140, 180
        /// Parameter: ms-lang
        ///     Values: The BCP 47 language tag set in the notification xml, or if omitted, the current preferred language of the user
        /// </summary>
        bool AddImageQuery { get; set; }     
    }

    namespace ToastContent
    {
        /// <summary>
        /// Type representing the toast notification audio properties which is contained within
        /// a toast notification content object.
        /// </summary>
        interface IToastAudio
        {
            /// <summary>
            /// The audio content that should be played when the toast is shown.
            /// </summary>
            ToastAudioContent Content { get; set; }

            /// <summary>
            /// Whether the audio should loop.  If this property is set to true, the toast audio content
            /// must be a looping sound.
            /// </summary>
            bool Loop { get; set; }
        }

        /// <summary>
        /// Base toast notification content interface.
        /// </summary>
        interface IToastNotificationContent : INotificationContent
        {
            /// <summary>
            /// Whether strict validation should be applied when the Xml or notification object is created,
            /// and when some of the properties are assigned.
            /// </summary>
            bool StrictValidation { get; set; }

            /// <summary>
            /// The language of the content being displayed.  The language should be specified using the
            /// abbreviated language code as defined by BCP 47.
            /// </summary>
            string Lang { get; set; }

            /// <summary>
            /// The BaseUri that should be used for image locations.  Relative image locations use this
            /// field as their base Uri.  The BaseUri must begin with http://, https://, ms-appx:///, or 
            /// ms-appdata:///local/.
            /// </summary>
            string BaseUri { get; set; }

            /// <summary>
            /// Controls if query strings that denote the client configuration of contrast, scale, and language setting should be appended to the Src
            /// If true, Windows will append query strings onto images that exist in this template
            /// If false (the default, Windows will not append query strings onto images that exist in this template            
            /// Query string details:
            /// Parameter: ms-contrast
            ///     Values: standard, black, white
            /// Parameter: ms-scale
            ///     Values: 80, 100, 140, 180
            /// Parameter: ms-lang
            ///     Values: The BCP 47 language tag set in the notification xml, or if omitted, the current preferred language of the user
            /// </summary>
            bool AddImageQuery { get; set; }     

            /// <summary>
            /// The launch parameter passed into the Windows Store app when the toast is activated.
            /// </summary>
            string Launch { get; set; }

            /// <summary>
            /// The audio that should be played when the toast is displayed.
            /// </summary>
            IToastAudio Audio { get; }

            /// <summary>
            /// The length that the toast should be displayed on screen.
            /// </summary>
            ToastDuration Duration { get; set; }

#if !WINRT_NOT_PRESENT
            /// <summary>
            /// Creates a WinRT ToastNotification object based on the content.
            /// </summary>
            /// <returns>A WinRT ToastNotification object based on the content.</returns>
            ToastNotification CreateNotification();
#endif
        }

        /// <summary>
        /// The audio options that can be played while the toast is on screen.
        /// </summary>
        enum ToastAudioContent
        {
            /// <summary>
            /// The default toast audio sound.
            /// </summary>
            Default = 0,
            /// <summary>
            /// Audio that corresponds to new mail arriving.
            /// </summary>
            Mail,
            /// <summary>
            /// Audio that corresponds to a new SMS message arriving.
            /// </summary>
            SMS,
            /// <summary>
            /// Audio that corresponds to a new IM arriving.
            /// </summary>
            IM,
            /// <summary>
            /// Audio that corresponds to a reminder.
            /// </summary>
            Reminder,
            /// <summary>
            /// The default looping sound.  Audio that corresponds to a call.
            /// Only valid for toasts that are have the duration set to "Long".
            /// </summary>
            LoopingCall,
            /// <summary>
            /// Audio that corresponds to a call.
            /// Only valid for toasts that are have the duration set to "Long".
            /// </summary>
            LoopingCall2,
            /// <summary>
            /// Audio that corresponds to an alarm.
            /// Only valid for toasts that are have the duration set to "Long".
            /// </summary>
            LoopingAlarm,
            /// <summary>
            /// Audio that corresponds to an alarm.
            /// Only valid for toasts that are have the duration set to "Long".
            /// </summary>
            LoopingAlarm2,
            /// <summary>
            /// No audio should be played when the toast is displayed.
            /// </summary>
            Silent
        }

        /// <summary>
        /// The duration the toast should be displayed on screen.
        /// </summary>
        enum ToastDuration
        {
            /// <summary>
            /// Default behavior.  The toast will be on screen for a short amount of time.
            /// </summary>
            Short = 0,
            /// <summary>
            /// The toast will be on screen for a longer amount of time.
            /// </summary>
            Long
        }
    }
}
