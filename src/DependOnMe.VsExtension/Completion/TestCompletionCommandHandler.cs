﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;

namespace DependOnMe.VsExtension.Completion
{
    public sealed class TestCompletionCommandHandler : IOleCommandTarget
	{
		private readonly IOleCommandTarget _nextCommandHandler;
		private readonly ITextView _textView;
		private readonly SVsServiceProvider _provider;
	    private readonly ICompletionBroker _completionBroker;
	    private ICompletionSession _session;

		public TestCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, SVsServiceProvider provider, ICompletionBroker completionBroker)
		{
			_textView = textView;
			_provider = provider;
		    _completionBroker = completionBroker;

		    //add the command to the command chain
			textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
		}

	    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	    {
            ThreadHelper.ThrowIfNotOnUIThread();

	        return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

	    public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
		    ThreadHelper.ThrowIfNotOnUIThread();

            if (VsShellUtilities.IsInAutomationFunction(_provider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }

            //make a copy of this so we can look at it after forwarding some commands
            uint commandId = nCmdId;
			char typedChar = char.MinValue;
			//make sure the input is a char before getting it
			if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
			{
				typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
			}

			//check for a commit character
			if (nCmdId == (uint)VSConstants.VSStd2KCmdID.RETURN || 
			    nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB || 
			    char.IsWhiteSpace(typedChar) || 
			    char.IsPunctuation(typedChar))
			{
				//check for a a selection
				if (_session != null && !_session.IsDismissed)
				{
					//if the selection is fully selected, commit the current session
					if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
					{
						_session.Commit();
						//also, don't add the character to the buffer
						return VSConstants.S_OK;
					}

				    //if there is no selection, dismiss the session
				    _session.Dismiss();
				}
			}

			//pass along the command so the char is added to the buffer
			int retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
			bool handled = false;
			if (!typedChar.Equals(char.MinValue) && (char.IsLetterOrDigit(typedChar) || char.IsWhiteSpace(typedChar) || true))
			{
				if (_session == null || _session.IsDismissed) // If there is no active session, bring up completion
				{
					TriggerCompletion();
                    _session?.Filter();
                }
				else    //the completion session is already active, so just filter
				{
					_session.Filter();
				}
				handled = true;
			}
			else if (commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
				|| commandId == (uint)VSConstants.VSStd2KCmdID.DELETE)
			{
				if (_session != null && !_session.IsDismissed)
					_session.Filter();
				handled = true;
			}
			if (handled) return VSConstants.S_OK;

			return retVal;
		}

		private bool TriggerCompletion()
		{
			//the caret must be in a non-projection location 
			SnapshotPoint? caretPoint =
				_textView.Caret.Position.Point.GetPoint(
					textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
			if (!caretPoint.HasValue)
			{
				return false;
			}

			_session = _completionBroker.CreateCompletionSession
			(_textView,
				caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
				true);

			//subscribe to the Dismissed event on the session 
			_session.Dismissed += OnSessionDismissed;
			_session.Start();

			return true;
		}

		private void OnSessionDismissed(object sender, EventArgs e)
		{
			_session.Dismissed -= OnSessionDismissed;
			_session = null;
		}
	}
}
