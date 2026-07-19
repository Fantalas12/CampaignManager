import React, { useState, useEffect } from 'react';
import { MetadataNote, Note, Session } from '../backend';
import { useGetNotes, useGetSessions, useGetNoteAccess, useFindNoteAcrossSessions, useGetMetadataNoteDetails } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { ArrowLeft, Database, User, Link, Copy, CheckCircle, FileText, ExternalLink, Lock, Loader2, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';

interface LinkedSessionNoteInfo {
  noteId: string;
  noteName: string;
  sessionName: string;
  hasAccess: boolean;
  isOwner: boolean;
  loading: boolean;
}

interface MetadataNoteDetailViewProps {
  metadataNote: MetadataNote;
  creatorName?: string;
  onBack: () => void;
  onNavigateToNote?: (noteId: string) => void;
}

export default function MetadataNoteDetailView({ 
  metadataNote: initialMetadataNote, 
  creatorName,
  onBack,
  onNavigateToNote
}: MetadataNoteDetailViewProps) {
  const [metadataNote, setMetadataNote] = useState<MetadataNote>(initialMetadataNote);
  const [copied, setCopied] = useState(false);
  const [linkedNotesInfo, setLinkedNotesInfo] = useState<LinkedSessionNoteInfo[]>([]);
  const [loadingLinkedNotes, setLoadingLinkedNotes] = useState(true);
  const [accessError, setAccessError] = useState<string | null>(null);
  
  const getSessions = useGetSessions();
  const getNotes = useGetNotes();
  const getNoteAccess = useGetNoteAccess();
  const findNoteAcrossSessions = useFindNoteAcrossSessions();
  const getMetadataNoteDetails = useGetMetadataNoteDetails();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isOwner = metadataNote.owner.toString() === currentUserPrincipal?.toString();

  useEffect(() => {
    // Verify access to this metadata note details page
    verifyAccess();
    loadLinkedSessionNotesInfo();
  }, [metadataNote.linkedSessionNotes, currentUserPrincipal]);

  const verifyAccess = async () => {
    if (!currentUserPrincipal) {
      setAccessError('Authentication required');
      return;
    }

    // If user is the owner, they have access
    if (isOwner) {
      setAccessError(null);
      return;
    }

    try {
      // Try to get the metadata note details to verify access
      const details = await getMetadataNoteDetails.mutateAsync(metadataNote.id);
      if (details) {
        setMetadataNote(details);
        setAccessError(null);
      } else {
        setAccessError('You do not have permission to view this metadata note');
      }
    } catch (error: any) {
      console.error('Access verification failed:', error);
      setAccessError('You do not have permission to view this metadata note');
    }
  };

  const loadLinkedSessionNotesInfo = async () => {
    if (!currentUserPrincipal) {
      setLoadingLinkedNotes(false);
      return;
    }

    setLoadingLinkedNotes(true);
    
    // Initialize the linked notes info with loading state
    const initialInfo: LinkedSessionNoteInfo[] = metadataNote.linkedSessionNotes.map(noteId => ({
      noteId,
      noteName: noteId, // Use noteId as fallback name initially
      sessionName: 'Session Note',
      hasAccess: false,
      isOwner: false,
      loading: true
    }));
    
    setLinkedNotesInfo(initialInfo);

    // Process each linked session note to get its details and access info
    const updatedInfo = await Promise.all(
      metadataNote.linkedSessionNotes.map(async (noteId): Promise<LinkedSessionNoteInfo> => {
        try {
          // Try to get note access information to check permissions
          let hasReadAccess = false;
          let isOwner = false;
          let noteName = noteId;
          let sessionName = 'Session Note';
          
          try {
            const accessInfo = await getNoteAccess.mutateAsync(noteId);
            
            // Check if user has read access
            const userAccess = accessInfo.find(access => 
              access.participant.toString() === currentUserPrincipal.toString()
            );
            
            if (userAccess) {
              // Check for read permissions (Player Read = 1, GM Read = 2)
              hasReadAccess = (userAccess.permissions & 1) !== 0 || (userAccess.permissions & 2) !== 0;
            }
            
          } catch (error) {
            // If we can't get access info, the user might still be the owner
            // We'll check this during navigation
            console.log(`Could not get access info for note ${noteId}, will check ownership during navigation`);
          }

          return {
            noteId,
            noteName,
            sessionName,
            hasAccess: hasReadAccess,
            isOwner: false, // We'll determine ownership during navigation
            loading: false
          };
        } catch (error) {
          console.error(`Failed to load info for note ${noteId}:`, error);
          return {
            noteId,
            noteName: noteId,
            sessionName: 'Session Note',
            hasAccess: false,
            isOwner: false,
            loading: false
          };
        }
      })
    );

    setLinkedNotesInfo(updatedInfo);
    setLoadingLinkedNotes(false);
  };

  // Format JSON content for display
  const getFormattedContent = () => {
    try {
      const parsed = JSON.parse(metadataNote.content);
      return JSON.stringify(parsed, null, 2);
    } catch {
      // If JSON parsing fails, return raw content
      return metadataNote.content;
    }
  };

  const handleCopyContent = async () => {
    try {
      await navigator.clipboard.writeText(getFormattedContent());
      setCopied(true);
      toast.success('JSON content copied to clipboard!');
      setTimeout(() => setCopied(false), 2000);
    } catch (error) {
      toast.error('Failed to copy content to clipboard');
    }
  };

  const handleNavigateToSessionNote = async (noteInfo: LinkedSessionNoteInfo) => {
    // Use the same navigation logic as NoteDetailView:
    // Always attempt navigation - the note detail view will handle the final access control
    // and display appropriate content based on ownership and permissions
    if (onNavigateToNote) {
      onNavigateToNote(noteInfo.noteId);
    }
  };

  // If there's an access error, show access denied screen
  if (accessError) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Button
            onClick={onBack}
            variant="outline"
            size="sm"
            className="border-white/20 text-white hover:bg-white/10"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>
          <div className="flex items-center space-x-3 flex-1">
            <Lock className="h-8 w-8 text-red-400" />
            <div>
              <h1 className="text-3xl font-bold text-white">Access Denied</h1>
              <p className="text-purple-200">Metadata Note Access Restricted</p>
            </div>
          </div>
        </div>

        <Card className="bg-white/10 backdrop-blur-sm border-white/20">
          <CardHeader>
            <CardTitle className="text-white flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-red-400" />
              Access Restricted
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-center py-8">
              <Lock className="h-16 w-16 text-red-400 mx-auto mb-4" />
              <h4 className="text-lg font-semibold text-white mb-2">No Access Permission</h4>
              <p className="text-purple-200 text-sm mb-4">
                {accessError}
              </p>
              <p className="text-purple-300 text-xs">
                Only the metadata note owner or users with write access to at least one of its linked session notes can view the details.
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center space-x-4">
        <Button
          onClick={onBack}
          variant="outline"
          size="sm"
          className="border-white/20 text-white hover:bg-white/10"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>
        <div className="flex items-center space-x-3 flex-1">
          <Database className="h-8 w-8 text-purple-400" />
          <div>
            <h1 className="text-3xl font-bold text-white">{metadataNote.name}</h1>
            <div className="flex items-center space-x-4 text-purple-200">
              <div className="flex items-center space-x-1">
                <User className="h-4 w-4" />
                <span>by {creatorName || 'Unknown'}</span>
                {isOwner && (
                  <span className="text-xs text-amber-400 ml-1">(You)</span>
                )}
              </div>
              <Badge variant="outline" className="text-xs text-purple-300 border-purple-400/30">
                Metadata Note
              </Badge>
            </div>
          </div>
        </div>
        <Button
          onClick={handleCopyContent}
          variant="outline"
          size="sm"
          className="border-white/20 text-white hover:bg-white/10"
        >
          {copied ? (
            <>
              <CheckCircle className="h-4 w-4 mr-2 text-green-400" />
              Copied!
            </>
          ) : (
            <>
              <Copy className="h-4 w-4 mr-2" />
              Copy JSON
            </>
          )}
        </Button>
      </div>

      {/* Content Substitution Info */}
      <Card className="bg-blue-500/10 backdrop-blur-sm border-blue-400/20">
        <CardHeader>
          <CardTitle className="text-blue-300 flex items-center gap-2">
            <Database className="h-5 w-5" />
            Content Substitution
          </CardTitle>
          <CardDescription className="text-blue-200">
            This metadata note provides content substitution for session notes. Session notes can reference 
            this data using placeholders like <code className="bg-black/30 px-1 py-0.5 rounded text-blue-100">&lt;---field---&gt;</code> 
            for automatic content replacement.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 text-sm text-blue-200">
            <p><strong>Example placeholders:</strong></p>
            <ul className="list-disc list-inside space-y-1 ml-4">
              <li><code className="bg-black/30 px-1 py-0.5 rounded">&lt;---character.name---&gt;</code> - Access nested fields</li>
              <li><code className="bg-black/30 px-1 py-0.5 rounded">&lt;---items[0].name---&gt;</code> - Access array elements</li>
              <li><code className="bg-black/30 px-1 py-0.5 rounded">&lt;---location---&gt;</code> - Access top-level fields</li>
            </ul>
          </div>
        </CardContent>
      </Card>

      {/* Linked Session Notes */}
      {metadataNote.linkedSessionNotes.length > 0 && (
        <Card className="bg-white/10 backdrop-blur-sm border-white/20">
          <CardHeader>
            <CardTitle className="text-white flex items-center gap-2">
              <Link className="h-5 w-5 text-blue-400" />
              Linked Session Notes ({metadataNote.linkedSessionNotes.length})
            </CardTitle>
            <CardDescription className="text-purple-200">
              This metadata note is linked to the following session notes and provides content for them. Click on any note to navigate to it.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {loadingLinkedNotes ? (
              <div className="flex items-center justify-center py-4">
                <Loader2 className="h-5 w-5 animate-spin text-purple-400" />
                <span className="ml-2 text-purple-200">Loading linked session notes...</span>
              </div>
            ) : (
              <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
                {linkedNotesInfo.map((noteInfo) => {
                  return (
                    <div
                      key={noteInfo.noteId}
                      className="flex items-center p-3 bg-white/5 rounded-lg border border-white/10 transition-colors hover:bg-white/10 cursor-pointer"
                      onClick={() => handleNavigateToSessionNote(noteInfo)}
                    >
                      <div className="flex items-center gap-2 flex-1 min-w-0">
                        {noteInfo.loading ? (
                          <Loader2 className="h-4 w-4 animate-spin text-purple-400 flex-shrink-0" />
                        ) : (
                          <FileText className="h-4 w-4 flex-shrink-0 text-blue-400" />
                        )}
                        <div className="min-w-0 flex-1">
                          <div className="font-medium text-sm truncate text-white">
                            {noteInfo.noteName}
                          </div>
                          <div className="text-xs truncate text-purple-200">
                            {noteInfo.sessionName}
                          </div>
                        </div>
                        {!noteInfo.loading && (
                          <ExternalLink className="h-4 w-4 text-blue-400 flex-shrink-0 ml-2" />
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* JSON Content */}
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader>
          <CardTitle className="text-white flex items-center gap-2">
            <Database className="h-5 w-5 text-purple-400" />
            JSON Content
          </CardTitle>
          <CardDescription className="text-purple-200">
            Structured data that is automatically substituted into session notes when they contain matching placeholders.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ScrollArea className="h-[400px] w-full">
            <pre className="bg-black/30 border border-white/10 rounded-lg p-4 text-sm text-white font-mono overflow-x-auto">
              {getFormattedContent()}
            </pre>
          </ScrollArea>
        </CardContent>
      </Card>
    </div>
  );
}
