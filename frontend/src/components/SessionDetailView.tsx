import React, { useState, useEffect } from 'react';
import { Session, Participant, SessionPlayer, Role, Note } from '../backend';
import { useGetSessionPlayers, useGetUserProfile, useUpdateSession, useDeleteSession, useGetNotes, useFindNoteAcrossSessions, useDeleteNote } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import EditSessionDialog from './EditSessionDialog';
import SessionPlayersDialog from './SessionPlayersDialog';
import CreateNoteDialog from './CreateNoteDialog';
import NoteCard from './NoteCard';
import NoteDetailView from './NoteDetailView';
import SessionNotesAndMetadataDashboard from './SessionNotesAndMetadataDashboard';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Badge } from '@/components/ui/badge';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { ArrowLeft, Calendar, Users, Clock, User, Edit, Loader2, FileText, Crown, Gamepad2, Eye, Trash2, AlertTriangle, Plus, Info, Database } from 'lucide-react';
import { toast } from 'sonner';

interface SessionDetailViewProps {
  session: Session;
  campaignParticipants: Array<Participant & { name?: string }>;
  onBack: () => void;
  onSessionUpdated?: () => void;
}

interface SessionPlayerWithProfile extends SessionPlayer {
  name?: string;
}

interface NoteWithProfile extends Note {
  creatorName?: string;
}

type ViewState = 
  | { type: 'session' }
  | { type: 'note'; note: NoteWithProfile }
  | { type: 'dashboard' };

export default function SessionDetailView({ 
  session, 
  campaignParticipants, 
  onBack, 
  onSessionUpdated 
}: SessionDetailViewProps) {
  const [sessionPlayers, setSessionPlayers] = useState<SessionPlayerWithProfile[]>([]);
  const [notes, setNotes] = useState<NoteWithProfile[]>([]);
  const [viewState, setViewState] = useState<ViewState>({ type: 'session' });
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isSessionPlayersDialogOpen, setIsSessionPlayersDialogOpen] = useState(false);
  const [isCreateNoteDialogOpen, setIsCreateNoteDialogOpen] = useState(false);
  const [loadingSessionPlayers, setLoadingSessionPlayers] = useState(false);
  const [loadingNotes, setLoadingNotes] = useState(false);
  
  const getSessionPlayers = useGetSessionPlayers();
  const getNotes = useGetNotes();
  const getUserProfile = useGetUserProfile();
  const deleteSession = useDeleteSession();
  const deleteNote = useDeleteNote();
  const findNoteAcrossSessions = useFindNoteAcrossSessions();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isSessionOwner = session.creator.toString() === currentUserPrincipal?.toString();

  // Find the creator's name
  const creator = campaignParticipants.find(p => p.principal.toString() === session.creator.toString());
  const creatorName = creator?.name || 'Unknown GM';

  // Get current user's role in the campaign
  const currentUserParticipant = campaignParticipants.find(p => 
    p.principal.toString() === currentUserPrincipal?.toString()
  );

  useEffect(() => {
    loadSessionPlayers();
    loadNotes();
  }, [session.id]);

  const loadSessionPlayers = async () => {
    setLoadingSessionPlayers(true);
    try {
      const playerList = await getSessionPlayers.mutateAsync(session.id);
      const playersWithProfiles = await Promise.all(
        playerList.map(async (player) => {
          try {
            const profile = await getUserProfile.mutateAsync(player.participant);
            return {
              ...player,
              name: profile?.name,
            };
          } catch {
            return player;
          }
        })
      );
      setSessionPlayers(playersWithProfiles);
    } catch (error) {
      console.error('Failed to load session players:', error);
      setSessionPlayers([]);
    } finally {
      setLoadingSessionPlayers(false);
    }
  };

  const loadNotes = async () => {
    setLoadingNotes(true);
    try {
      const noteList = await getNotes.mutateAsync(session.id);
      const notesWithProfiles = await Promise.all(
        noteList.map(async (note) => {
          try {
            const profile = await getUserProfile.mutateAsync(note.creator);
            return {
              ...note,
              creatorName: profile?.name,
            };
          } catch {
            return note;
          }
        })
      );
      setNotes(notesWithProfiles);
    } catch (error) {
      console.error('Failed to load notes:', error);
      setNotes([]);
    } finally {
      setLoadingNotes(false);
    }
  };

  // Format the date
  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    } catch {
      return dateString;
    }
  };

  const handleSessionUpdated = () => {
    onSessionUpdated?.();
  };

  const handleSessionPlayersUpdated = () => {
    loadSessionPlayers();
  };

  const handleNoteCreated = () => {
    loadNotes();
  };

  const handleNoteUpdated = () => {
    loadNotes();
  };

  const handleNoteDeleted = () => {
    if (viewState.type === 'note') {
      deleteNote.mutate(viewState.note.id, {
        onSuccess: () => {
          setViewState({ type: 'session' });
          loadNotes(); // Refresh the notes list
        }
      });
    }
  };

  const handleDeleteSession = () => {
    deleteSession.mutate(session.id, {
      onSuccess: () => {
        setIsDeleteDialogOpen(false);
        onBack(); // Go back to campaign after successful deletion
        onSessionUpdated?.(); // Update the session list
      }
    });
  };

  const handleNoteClick = (note: Note) => {
    // Find the note with profile info if it exists in our current notes
    const noteWithProfile = notes.find(n => n.id === note.id);
    setViewState({ 
      type: 'note', 
      note: noteWithProfile || { ...note, creatorName: undefined } 
    });
  };

  const handleBackToSession = () => {
    setViewState({ type: 'session' });
  };

  const handleViewDashboard = () => {
    setViewState({ type: 'dashboard' });
  };

  const handleBackFromDashboard = () => {
    setViewState({ type: 'session' });
  };

  const handleNavigateToNote = async (noteId: string) => {
    // First, check if the note is in the current session
    const currentSessionNote = notes.find(note => note.id === noteId);
    if (currentSessionNote) {
      setViewState({ type: 'note', note: currentSessionNote });
      return;
    }

    // If not found in current session, search across all sessions in the campaign
    try {
      const result = await findNoteAcrossSessions.mutateAsync({
        campaignId: session.campaignId,
        noteId: noteId
      });

      if (result) {
        // Load the creator's profile for the found note
        try {
          const profile = await getUserProfile.mutateAsync(result.note.creator);
          const noteWithProfile: NoteWithProfile = {
            ...result.note,
            creatorName: profile?.name,
          };
          setViewState({ type: 'note', note: noteWithProfile });
          
          // Show a toast to inform the user they've navigated to a different session
          if (result.session.id !== session.id) {
            toast.info(`Navigated to note "${result.note.name}" from session "${result.session.name}"`);
          }
        } catch {
          // If we can't get the profile, still show the note
          const noteWithProfile: NoteWithProfile = {
            ...result.note,
            creatorName: undefined,
          };
          setViewState({ type: 'note', note: noteWithProfile });
        }
      } else {
        toast.error('Note not found or you do not have access to it');
      }
    } catch (error) {
      console.error('Failed to find note across sessions:', error);
      toast.error('Failed to navigate to the linked note');
    }
  };

  const getRoleIcon = (role: Role) => {
    switch (role) {
      case Role.gm:
        return <Crown className="h-4 w-4 text-amber-400" />;
      case Role.player:
        return <Gamepad2 className="h-4 w-4 text-blue-400" />;
      case Role.both:
        return (
          <div className="flex items-center space-x-1">
            <Crown className="h-3 w-3 text-amber-400" />
            <Gamepad2 className="h-3 w-3 text-blue-400" />
          </div>
        );
    }
  };

  // Handle different view states
  if (viewState.type === 'note') {
    return (
      <NoteDetailView 
        note={viewState.note} 
        creatorName={viewState.note.creatorName}
        campaignParticipants={campaignParticipants}
        currentUserRole={currentUserParticipant?.role}
        onBack={handleBackToSession}
        onNoteUpdated={handleNoteUpdated}
        onNoteDeleted={handleNoteDeleted}
        onNavigateToNote={handleNavigateToNote}
      />
    );
  }

  if (viewState.type === 'dashboard') {
    return (
      <SessionNotesAndMetadataDashboard
        session={session}
        campaignParticipants={campaignParticipants}
        onBack={handleBackFromDashboard}
        onNoteClick={handleNoteClick}
      />
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
          Back to Campaign
        </Button>
        <div className="flex items-center space-x-3 flex-1">
          <Clock className="h-8 w-8 text-blue-400" />
          <div>
            <h1 className="text-3xl font-bold text-white">{session.name}</h1>
            <div className="flex items-center space-x-4 text-purple-200">
              <div className="flex items-center space-x-1">
                <Calendar className="h-4 w-4" />
                <span>{formatDate(session.date)}</span>
              </div>
              <div className="flex items-center space-x-1">
                <User className="h-4 w-4" />
                <span>by {creatorName}</span>
                {isSessionOwner && (
                  <span className="text-xs text-amber-400 ml-1">(You)</span>
                )}
              </div>
            </div>
          </div>
        </div>
        {/* Session Management Actions - Only for session owners */}
        {isSessionOwner && (
          <div className="flex items-center space-x-2">
            <Button
              onClick={() => setIsEditDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Edit className="h-4 w-4 mr-2" />
              Edit Session
            </Button>
            <Button
              onClick={() => setIsDeleteDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete Session
            </Button>
          </div>
        )}
      </div>

      {/* Session Details */}
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader>
          <CardTitle className="text-white flex items-center gap-2">
            <Clock className="h-5 w-5 text-blue-400" />
            Session Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="text-sm font-medium text-purple-300 mb-2">Description</h4>
            <div 
              className="text-white whitespace-pre-wrap native-html-content"
              dangerouslySetInnerHTML={{ __html: session.description || 'No description provided.' }}
            />
          </div>
          <Separator className="bg-white/20" />
          <div className="flex items-center justify-between">
            <div>
              <h4 className="text-sm font-medium text-purple-300 mb-2">Session Players</h4>
              <div className="flex items-center space-x-4">
                <span className="text-white">{sessionPlayers.length} player{sessionPlayers.length === 1 ? '' : 's'}</span>
                {loadingSessionPlayers ? (
                  <Loader2 className="h-4 w-4 animate-spin text-purple-400" />
                ) : (
                  <div className="flex items-center space-x-2">
                    {sessionPlayers.slice(0, 3).map((player, index) => (
                      <div key={player.participant.toString()} className="flex items-center space-x-1">
                        <span className="text-xs text-purple-200">
                          {player.name || 'Unknown'}
                        </span>
                      </div>
                    ))}
                    {sessionPlayers.length > 3 && (
                      <span className="text-xs text-purple-300">+{sessionPlayers.length - 3} more</span>
                    )}
                  </div>
                )}
              </div>
            </div>
            <Button
              onClick={() => setIsSessionPlayersDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              {isSessionOwner ? <Users className="h-4 w-4 mr-2" /> : <Eye className="h-4 w-4 mr-2" />}
              {isSessionOwner ? 'Manage Players' : 'View Players'}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Session Notes Section */}
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-white flex items-center gap-2">
              <FileText className="h-5 w-5 text-green-400" />
              Session Notes ({notes.length})
            </CardTitle>
            <div className="flex items-center space-x-2">
              <Button
                onClick={handleViewDashboard}
                variant="outline"
                size="sm"
                className="border-white/20 text-white hover:bg-white/10"
              >
                <Database className="h-4 w-4 mr-2" />
                Notes & Metadata Dashboard
              </Button>
              {isSessionOwner && (
                <Button
                  onClick={() => setIsCreateNoteDialogOpen(true)}
                  size="sm"
                  className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white border-0"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Create Note
                </Button>
              )}
            </div>
          </div>
          <CardDescription className="text-purple-200 flex items-start gap-2">
            <div className="flex-1">
              {isSessionOwner 
                ? "Create and manage notes for this session with HTML content support. Access the dashboard to view both session notes and metadata notes together."
                : "View notes created by the session owner for this session based on your access permissions. Content is rendered with HTML support for enhanced formatting."
              }
            </div>
            {!isSessionOwner && (
              <div className="flex items-center gap-1 text-xs text-amber-300 bg-amber-500/10 px-2 py-1 rounded border border-amber-400/20">
                <Info className="h-3 w-3" />
                <span>Access controlled</span>
              </div>
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loadingNotes ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
              <span className="ml-2 text-purple-200">Loading notes...</span>
            </div>
          ) : notes.length === 0 ? (
            <div className="text-center py-12">
              <FileText className="h-16 w-16 text-purple-400 mx-auto mb-4" />
              <h4 className="text-lg font-semibold text-white mb-2">No Notes Yet</h4>
              <p className="text-purple-200 mb-4 text-sm">
                {isSessionOwner 
                  ? "Create your first note to start documenting this session."
                  : "Notes will appear here when the session owner creates them and grants you access."
                }
              </p>
              {!isSessionOwner && (
                <div className="bg-amber-500/10 border border-amber-400/20 rounded-lg p-4 max-w-md mx-auto">
                  <div className="flex items-center justify-center gap-2 text-amber-300 mb-2">
                    <Crown className="h-4 w-4" />
                    <span className="text-sm font-medium">Session Owner Access Required</span>
                  </div>
                  <p className="text-xs text-amber-200">
                    Only the session owner can create notes. Notes you can access will appear here based on the permissions granted to you.
                  </p>
                </div>
              )}
              {isSessionOwner && (
                <Button
                  onClick={() => setIsCreateNoteDialogOpen(true)}
                  size="sm"
                  className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white border-0"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Create First Note
                </Button>
              )}
            </div>
          ) : (
            <ScrollArea className="h-[400px] pr-4">
              <div className="space-y-4">
                {notes.map((note) => (
                  <NoteCard 
                    key={note.id} 
                    note={note} 
                    creatorName={note.creatorName}
                    campaignParticipants={campaignParticipants}
                    currentUserRole={currentUserParticipant?.role}
                    onNoteClick={handleNoteClick}
                  />
                ))}
              </div>
            </ScrollArea>
          )}
        </CardContent>
      </Card>

      {/* Dialogs */}
      {isSessionOwner && (
        <>
          <EditSessionDialog
            session={session}
            open={isEditDialogOpen}
            onOpenChange={setIsEditDialogOpen}
            onSessionUpdated={handleSessionUpdated}
          />

          <CreateNoteDialog
            session={session}
            open={isCreateNoteDialogOpen}
            onOpenChange={setIsCreateNoteDialogOpen}
            onNoteCreated={handleNoteCreated}
          />

          <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
            <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white max-w-md">
              <AlertDialogHeader className="space-y-4">
                <div className="flex items-center justify-center">
                  <div className="bg-red-500/20 p-3 rounded-full">
                    <AlertTriangle className="h-8 w-8 text-red-400" />
                  </div>
                </div>
                <AlertDialogTitle className="text-center text-xl font-bold text-white">
                  Delete Session
                </AlertDialogTitle>
                <AlertDialogDescription className="text-center text-purple-200 space-y-2">
                  <p className="font-medium">
                    Are you sure you want to delete <span className="text-white font-semibold">"{session.name}"</span>?
                  </p>
                  <p className="text-sm">
                    <span className="font-medium text-blue-300">Date:</span> {formatDate(session.date)}
                  </p>
                  <p className="text-sm text-red-300">
                    This action cannot be undone. The session and all its notes will be permanently removed from the campaign.
                  </p>
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
                <AlertDialogCancel 
                  className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
                  disabled={deleteSession.isPending}
                >
                  Cancel
                </AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleDeleteSession}
                  disabled={deleteSession.isPending}
                  className="bg-red-600 hover:bg-red-700 text-white border-0 order-1 sm:order-2"
                >
                  {deleteSession.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Deleting...
                    </>
                  ) : (
                    <>
                      <Trash2 className="h-4 w-4 mr-2" />
                      Delete Session
                    </>
                  )}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </>
      )}

      <SessionPlayersDialog
        session={session}
        campaignParticipants={campaignParticipants}
        open={isSessionPlayersDialogOpen}
        onOpenChange={setIsSessionPlayersDialogOpen}
        onSessionPlayersUpdated={handleSessionPlayersUpdated}
      />
    </div>
  );
}
