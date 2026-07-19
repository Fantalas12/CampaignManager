import React, { useState, useEffect } from 'react';
import { Session, Note, MetadataNote, Participant, Role } from '../backend';
import { useGetNotes, useGetUserProfile, useGetMetadataNotes, useCheckWriteAccessInCampaign } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import NoteCard from './NoteCard';
import MetadataNoteCard from './MetadataNoteCard';
import CreateMetadataNoteDialog from './CreateMetadataNoteDialog';
import MetadataNoteDetailView from './MetadataNoteDetailView';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, FileText, Database, Plus, Loader2, Info, Crown } from 'lucide-react';

interface SessionNotesAndMetadataDashboardProps {
  session: Session;
  campaignParticipants: Array<Participant & { name?: string }>;
  onBack: () => void;
  onNoteClick: (note: Note) => void;
}

interface NoteWithProfile extends Note {
  creatorName?: string;
}

interface MetadataNoteWithProfile extends MetadataNote {
  creatorName?: string;
}

export default function SessionNotesAndMetadataDashboard({ 
  session, 
  campaignParticipants, 
  onBack,
  onNoteClick 
}: SessionNotesAndMetadataDashboardProps) {
  const [notes, setNotes] = useState<NoteWithProfile[]>([]);
  const [metadataNotes, setMetadataNotes] = useState<MetadataNoteWithProfile[]>([]);
  const [selectedMetadataNote, setSelectedMetadataNote] = useState<MetadataNoteWithProfile | null>(null);
  const [isCreateMetadataDialogOpen, setIsCreateMetadataDialogOpen] = useState(false);
  const [loadingNotes, setLoadingNotes] = useState(false);
  const [canCreateMetadata, setCanCreateMetadata] = useState(false);
  const [checkingAccess, setCheckingAccess] = useState(false);
  
  const getNotes = useGetNotes();
  const getUserProfile = useGetUserProfile();
  const { data: allMetadataNotes, isLoading: metadataLoading, error: metadataError } = useGetMetadataNotes();
  const checkWriteAccess = useCheckWriteAccessInCampaign();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isSessionOwner = session.creator.toString() === currentUserPrincipal?.toString();

  // Get current user's role in the campaign
  const currentUserParticipant = campaignParticipants.find(p => 
    p.principal.toString() === currentUserPrincipal?.toString()
  );

  useEffect(() => {
    loadNotes();
    checkMetadataCreationAccess();
  }, [session.id]);

  // Process metadata notes whenever allMetadataNotes changes
  useEffect(() => {
    if (allMetadataNotes) {
      loadMetadataNotes();
    }
  }, [allMetadataNotes]);

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

  const loadMetadataNotes = async () => {
    if (allMetadataNotes) {
      try {
        const metadataWithProfiles = await Promise.all(
          allMetadataNotes.map(async (metadataNote) => {
            try {
              const profile = await getUserProfile.mutateAsync(metadataNote.owner);
              return {
                ...metadataNote,
                creatorName: profile?.name,
              };
            } catch {
              return metadataNote;
            }
          })
        );
        setMetadataNotes(metadataWithProfiles);
      } catch (error) {
        console.error('Failed to process metadata notes:', error);
        setMetadataNotes([]);
      }
    }
  };

  const checkMetadataCreationAccess = async () => {
    setCheckingAccess(true);
    try {
      // Session owners can always create metadata notes, regardless of note permissions
      if (isSessionOwner) {
        setCanCreateMetadata(true);
      } else {
        // For non-session owners, check if they have write access to any note in the campaign
        const hasAccess = await checkWriteAccess.mutateAsync(session.campaignId);
        setCanCreateMetadata(hasAccess);
      }
    } catch (error) {
      console.error('Failed to check write access:', error);
      setCanCreateMetadata(isSessionOwner); // Fallback to session owner check
    } finally {
      setCheckingAccess(false);
    }
  };

  const handleMetadataNoteCreated = () => {
    // The useGetMetadataNotes query will automatically refetch due to invalidation
    // No need to manually reload
  };

  const handleMetadataNoteClick = (metadataNote: MetadataNoteWithProfile) => {
    setSelectedMetadataNote(metadataNote);
  };

  const handleBackFromMetadataNote = () => {
    setSelectedMetadataNote(null);
  };

  // If a metadata note is selected, show its detail view
  if (selectedMetadataNote) {
    return (
      <MetadataNoteDetailView 
        metadataNote={selectedMetadataNote}
        creatorName={selectedMetadataNote.creatorName}
        onBack={handleBackFromMetadataNote}
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
          Back to Session
        </Button>
        <div className="flex items-center space-x-3 flex-1">
          <FileText className="h-8 w-8 text-green-400" />
          <div>
            <h1 className="text-3xl font-bold text-white">Notes Dashboard</h1>
            <p className="text-purple-200">Session: {session.name}</p>
          </div>
        </div>
        {canCreateMetadata && (
          <Button
            onClick={() => setIsCreateMetadataDialogOpen(true)}
            size="sm"
            className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            disabled={checkingAccess}
          >
            {checkingAccess ? (
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
            ) : (
              <Plus className="h-4 w-4 mr-2" />
            )}
            Create Metadata Note
          </Button>
        )}
      </div>

      {/* Access Information */}
      {!canCreateMetadata && !checkingAccess && (
        <Card className="bg-amber-500/10 backdrop-blur-sm border-amber-400/30">
          <CardHeader>
            <CardTitle className="text-amber-300 flex items-center gap-2">
              <Info className="h-5 w-5" />
              Metadata Note Creation
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-amber-200 text-sm">
              <strong>Session Owner or Write Permission Required:</strong> You need to be the session owner 
              or have "Write" permission on at least one session note within this campaign to create metadata notes. 
              Session owners can always create metadata notes for their sessions, while other users need write access 
              to session notes for enhanced content management capabilities.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Notes and Metadata Tabs */}
      <Tabs defaultValue="session-notes" className="w-full">
        <TabsList className="grid w-full grid-cols-2 bg-white/10">
          <TabsTrigger 
            value="session-notes" 
            className="data-[state=active]:bg-white/20 flex items-center gap-2"
          >
            <FileText className="h-4 w-4" />
            Session Notes ({notes.length})
          </TabsTrigger>
          <TabsTrigger 
            value="metadata-notes" 
            className="data-[state=active]:bg-white/20 flex items-center gap-2"
          >
            <Database className="h-4 w-4" />
            Metadata Notes ({metadataNotes.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="session-notes" className="space-y-6 mt-6">
          <Card className="bg-white/10 backdrop-blur-sm border-white/20">
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="text-white flex items-center gap-2">
                  <FileText className="h-5 w-5 text-green-400" />
                  Session Notes ({notes.length})
                </CardTitle>
              </div>
              <CardDescription className="text-purple-200 flex items-start gap-2">
                <div className="flex-1">
                  {isSessionOwner 
                    ? "Session notes with HTML content rendering support. Click on any note to view its content and manage access permissions."
                    : "View session notes based on your access permissions. Content is rendered with HTML support for enhanced formatting."
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
                  <span className="ml-2 text-purple-200">Loading session notes...</span>
                </div>
              ) : notes.length === 0 ? (
                <div className="text-center py-12">
                  <FileText className="h-16 w-16 text-purple-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">No Session Notes</h4>
                  <p className="text-purple-200 mb-4 text-sm">
                    {isSessionOwner 
                      ? "Create your first session note to start documenting this session."
                      : "Session notes will appear here when the session owner creates them and grants you access."
                    }
                  </p>
                  {!isSessionOwner && (
                    <div className="bg-amber-500/10 border border-amber-400/20 rounded-lg p-4 max-w-md mx-auto">
                      <div className="flex items-center justify-center gap-2 text-amber-300 mb-2">
                        <Crown className="h-4 w-4" />
                        <span className="text-sm font-medium">Session Owner Access Required</span>
                      </div>
                      <p className="text-xs text-amber-200">
                        Only the session owner can create session notes. Notes you can access will appear here based on the permissions granted to you.
                      </p>
                    </div>
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
                        onNoteClick={onNoteClick}
                      />
                    ))}
                  </div>
                </ScrollArea>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="metadata-notes" className="space-y-6 mt-6">
          <Card className="bg-white/10 backdrop-blur-sm border-white/20">
            <CardHeader>
              <CardTitle className="text-white flex items-center gap-2">
                <Database className="h-5 w-5 text-purple-400" />
                Metadata Notes ({metadataNotes.length})
              </CardTitle>
              <CardDescription className="text-purple-200">
                Structured JSON metadata notes that can be linked to session notes for content substitution using placeholders.
                {isSessionOwner && (
                  <span className="block mt-1 text-green-300">
                    <Crown className="h-3 w-3 inline mr-1" />
                    As session owner, you can always create metadata notes for this session.
                  </span>
                )}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {metadataLoading ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                  <span className="ml-2 text-purple-200">Loading metadata notes...</span>
                </div>
              ) : metadataError ? (
                <div className="text-center py-12">
                  <Database className="h-16 w-16 text-red-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">Error Loading Metadata Notes</h4>
                  <p className="text-red-200 text-sm">
                    Failed to load metadata notes. Please try refreshing the page.
                  </p>
                </div>
              ) : metadataNotes.length === 0 ? (
                <div className="text-center py-12">
                  <Database className="h-16 w-16 text-purple-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">No Metadata Notes</h4>
                  <p className="text-purple-200 mb-4 text-sm">
                    {canCreateMetadata 
                      ? "Create your first metadata note to store structured JSON data for your campaign."
                      : "Metadata notes will appear here when users with appropriate permissions create them."
                    }
                  </p>
                </div>
              ) : (
                <ScrollArea className="h-[400px] pr-4">
                  <div className="space-y-4">
                    {metadataNotes.map((metadataNote) => (
                      <MetadataNoteCard 
                        key={metadataNote.id} 
                        metadataNote={metadataNote} 
                        creatorName={metadataNote.creatorName}
                        onMetadataNoteClick={handleMetadataNoteClick}
                      />
                    ))}
                  </div>
                </ScrollArea>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Create Metadata Note Dialog */}
      <CreateMetadataNoteDialog
        open={isCreateMetadataDialogOpen}
        onOpenChange={setIsCreateMetadataDialogOpen}
        onMetadataNoteCreated={handleMetadataNoteCreated}
        campaignId={session.campaignId}
        isSessionOwner={isSessionOwner}
      />
    </div>
  );
}
