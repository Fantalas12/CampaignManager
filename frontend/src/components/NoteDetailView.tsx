import React, { useState, useEffect } from 'react';
import { Note, LinkedNoteDetails, MetadataNote, Participant, Role } from '../backend';
import { useGetLinkedNotes, useGetLinkedMetadataNotes, useCheckMetadataNoteAccess, useUnlinkMetadataNote, useGetSessions } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import EditNoteDialog from './EditNoteDialog';
import ManageNoteAccessDialog from './ManageNoteAccessDialog';
import NoteLinkingDialog from './NoteLinkingDialog';
import TagInput from './TagInput';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { ArrowLeft, Edit, Trash2, Users, Link, Calendar, FileText, Database, Eye, Unlink, ExternalLink, Ban, Loader2, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';
import { processContentSubstitution } from '../lib/dynamicContentSubstitution';

interface NoteDetailViewProps {
  note: Note;
  creatorName?: string;
  campaignParticipants: Array<Participant & { name?: string }>;
  currentUserRole?: Role;
  onBack: () => void;
  onNoteUpdated: () => void;
  onNoteDeleted: () => void;
  onNavigateToNote?: (noteId: string) => void;
}

export default function NoteDetailView({ 
  note, 
  creatorName, 
  campaignParticipants, 
  currentUserRole,
  onBack, 
  onNoteUpdated, 
  onNoteDeleted,
  onNavigateToNote 
}: NoteDetailViewProps) {
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isAccessDialogOpen, setIsAccessDialogOpen] = useState(false);
  const [isLinkingDialogOpen, setIsLinkingDialogOpen] = useState(false);
  const [linkedNotes, setLinkedNotes] = useState<LinkedNoteDetails[]>([]);
  const [linkedMetadataNotes, setLinkedMetadataNotes] = useState<MetadataNote[]>([]);
  const [metadataAccessMap, setMetadataAccessMap] = useState<Map<string, boolean>>(new Map());
  const [loadingLinkedNotes, setLoadingLinkedNotes] = useState(false);
  const [loadingLinkedMetadata, setLoadingLinkedMetadata] = useState(false);
  const [unlinkingMetadataId, setUnlinkingMetadataId] = useState<string | null>(null);
  const [campaignId, setCampaignId] = useState<string>('');
  const [processedPlayerContent, setProcessedPlayerContent] = useState('');
  const [processedGmContent, setProcessedGmContent] = useState('');

  const { identity } = useInternetIdentity();
  const getLinkedNotes = useGetLinkedNotes();
  const getLinkedMetadataNotes = useGetLinkedMetadataNotes();
  const checkMetadataNoteAccess = useCheckMetadataNoteAccess();
  const unlinkMetadataNote = useUnlinkMetadataNote();
  const getSessions = useGetSessions();

  const currentUserPrincipal = identity?.getPrincipal();
  const isNoteOwner = note.creator.toString() === currentUserPrincipal?.toString();

  // Check if current user has write access (simplified check - in real app, this would check permissions)
  const hasWriteAccess = isNoteOwner; // For now, only note owners have write access

  // Check if current user can view player content
  const canViewPlayerContent = isNoteOwner || currentUserRole === 'player' || currentUserRole === 'both';
  
  // Check if current user can view GM content
  const canViewGmContent = isNoteOwner || currentUserRole === 'gm' || currentUserRole === 'both';

  useEffect(() => {
    loadLinkedNotes();
    loadLinkedMetadataNotes();
    findCampaignId();
  }, [note.id]);

  useEffect(() => {
    // Process content substitution when linked metadata notes change
    processContentWithSubstitution();
  }, [linkedMetadataNotes, note.playerContent, note.gmContent]);

  const findCampaignId = async () => {
    try {
      // We need to find which campaign this note belongs to by finding the session
      // This is a workaround since we don't have direct campaign access from the note
      // In a real implementation, we might store campaignId in the note or have a different approach
      
      // For now, we'll use a placeholder campaign ID
      // In the actual implementation, you would need to:
      // 1. Get the session that contains this note
      // 2. Get the campaign ID from that session
      setCampaignId('placeholder-campaign-id');
    } catch (error) {
      console.error('Failed to find campaign ID:', error);
    }
  };

  const processContentWithSubstitution = () => {
    if (linkedMetadataNotes.length === 0) {
      setProcessedPlayerContent(note.playerContent);
      setProcessedGmContent(note.gmContent);
      return;
    }

    // Process content substitution using the linked metadata notes directly
    const processedPlayer = processContentSubstitution(note.playerContent, linkedMetadataNotes);
    const processedGm = processContentSubstitution(note.gmContent, linkedMetadataNotes);

    setProcessedPlayerContent(processedPlayer);
    setProcessedGmContent(processedGm);
  };

  const loadLinkedNotes = async () => {
    setLoadingLinkedNotes(true);
    try {
      const links = await getLinkedNotes.mutateAsync(note.id);
      setLinkedNotes(links);
    } catch (error) {
      console.error('Failed to load linked notes:', error);
      setLinkedNotes([]);
    } finally {
      setLoadingLinkedNotes(false);
    }
  };

  const loadLinkedMetadataNotes = async () => {
    setLoadingLinkedMetadata(true);
    try {
      const linkedMetadata = await getLinkedMetadataNotes.mutateAsync(note.id);
      setLinkedMetadataNotes(linkedMetadata);
      
      // Check access for each metadata note
      const accessMap = new Map<string, boolean>();
      await Promise.all(
        linkedMetadata.map(async (metadataNote) => {
          try {
            const hasAccess = await checkMetadataNoteAccess.mutateAsync(metadataNote.id);
            accessMap.set(metadataNote.id, hasAccess);
          } catch (error) {
            console.error(`Failed to check access for metadata note ${metadataNote.id}:`, error);
            accessMap.set(metadataNote.id, false);
          }
        })
      );
      setMetadataAccessMap(accessMap);
    } catch (error) {
      console.error('Failed to load linked metadata notes:', error);
      setLinkedMetadataNotes([]);
    } finally {
      setLoadingLinkedMetadata(false);
    }
  };

  const handleNavigateToLinkedNote = (linkedNoteId: string) => {
    onNavigateToNote?.(linkedNoteId);
  };

  const handleNavigateToMetadataNote = (metadataNote: MetadataNote) => {
    const hasAccess = metadataAccessMap.get(metadataNote.id);
    if (hasAccess) {
      // For now, we'll show a toast since metadata note navigation might need special handling
      toast.info(`Navigating to metadata note: ${metadataNote.name}`);
      // TODO: Implement metadata note navigation if needed
      // onNavigateToMetadataNote?.(metadataNote.id);
    }
  };

  const handleUnlinkMetadata = async (metadataNote: MetadataNote) => {
    setUnlinkingMetadataId(metadataNote.id);
    try {
      await unlinkMetadataNote.mutateAsync({
        metadataNoteId: metadataNote.id,
        sessionNoteId: note.id,
      });
      await loadLinkedMetadataNotes(); // Refresh the linked metadata notes list
    } catch (error) {
      // Error is already handled by the mutation's onError
    } finally {
      setUnlinkingMetadataId(null);
    }
  };

  const handleLinksUpdated = () => {
    loadLinkedNotes();
    loadLinkedMetadataNotes();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button
            onClick={onBack}
            variant="outline"
            size="sm"
            className="border-white/20 text-white hover:bg-white/10"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div className="flex items-center space-x-3">
            <FileText className="h-8 w-8 text-green-400" />
            <div>
              <h1 className="text-3xl font-bold text-white">{note.name}</h1>
              <p className="text-purple-200">
                Created by {creatorName || 'Unknown'} 
                {note.inGameDateTime && (
                  <span className="ml-2 flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    {note.inGameDateTime}
                  </span>
                )}
              </p>
            </div>
          </div>
        </div>
        
        {hasWriteAccess && (
          <div className="flex items-center space-x-2">
            <Button
              onClick={() => setIsLinkingDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Link className="h-4 w-4 mr-2" />
              Manage Links
            </Button>
            <Button
              onClick={() => setIsEditDialogOpen(true)}
              variant="outline"
              size="sm"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Edit className="h-4 w-4 mr-2" />
              Edit
            </Button>
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  variant="outline"
                  size="sm"
                  className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white">
                <AlertDialogHeader>
                  <AlertDialogTitle className="flex items-center gap-2">
                    <AlertTriangle className="h-5 w-5 text-red-400" />
                    Delete Note
                  </AlertDialogTitle>
                  <AlertDialogDescription className="text-purple-200">
                    Are you sure you want to delete "{note.name}"? This action cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel className="bg-white/10 border-white/20 text-white hover:bg-white/20">
                    Cancel
                  </AlertDialogCancel>
                  <AlertDialogAction
                    onClick={onNoteDeleted}
                    className="bg-red-600 hover:bg-red-700 text-white border-0"
                  >
                    Delete Note
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </div>
        )}
      </div>

      {/* Tags */}
      {note.tags.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {note.tags.map((tag) => (
            <Badge key={tag} variant="secondary" className="bg-purple-500/20 text-purple-200 border-purple-400/30">
              {tag}
            </Badge>
          ))}
        </div>
      )}

      {/* Note Content */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Player Content */}
        {canViewPlayerContent && (
          <Card className="bg-white/10 backdrop-blur-sm border-white/20">
            <CardHeader>
              <CardTitle className="text-white flex items-center gap-2">
                <Users className="h-5 w-5 text-blue-400" />
                Player Content
              </CardTitle>
              <CardDescription className="text-purple-200">
                Content visible to players in this campaign.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div 
                className="prose prose-invert max-w-none native-html-content"
                dangerouslySetInnerHTML={{ __html: processedPlayerContent }}
              />
            </CardContent>
          </Card>
        )}

        {/* GM Content */}
        {canViewGmContent && (
          <Card className="bg-white/10 backdrop-blur-sm border-white/20">
            <CardHeader>
              <CardTitle className="text-white flex items-center gap-2">
                <Eye className="h-5 w-5 text-amber-400" />
                GM Content
              </CardTitle>
              <CardDescription className="text-purple-200">
                Content visible only to Game Masters.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div 
                className="prose prose-invert max-w-none native-html-content"
                dangerouslySetInnerHTML={{ __html: processedGmContent }}
              />
            </CardContent>
          </Card>
        )}
      </div>

      {/* Linked Notes Section */}
      {linkedNotes.length > 0 && (
        <Card className="bg-white/10 backdrop-blur-sm border-white/20">
          <CardHeader>
            <CardTitle className="text-white flex items-center gap-2">
              <Link className="h-5 w-5 text-green-400" />
              Linked Session Notes ({linkedNotes.length})
            </CardTitle>
            <CardDescription className="text-purple-200">
              Other session notes linked to this note.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {loadingLinkedNotes ? (
              <div className="flex items-center justify-center py-4">
                <Loader2 className="h-6 w-6 animate-spin text-purple-400" />
                <span className="ml-2 text-purple-200">Loading linked notes...</span>
              </div>
            ) : (
              <div className="space-y-3">
                {linkedNotes.map((linkedNote, index) => (
                  <div key={linkedNote.linkedNoteId}>
                    <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <FileText className="h-4 w-4 text-green-400 flex-shrink-0" />
                          <span className="font-medium text-white truncate">
                            {linkedNote.noteName}
                          </span>
                          {!linkedNote.hasReadAccess && !linkedNote.isOwner && (
                            <Badge variant="outline" className="text-xs text-red-400 border-red-400/30">
                              No Access
                            </Badge>
                          )}
                        </div>
                        <div className="flex items-center gap-3 text-sm text-purple-200">
                          <span className="truncate">Session: {linkedNote.sessionName}</span>
                          {linkedNote.tags.length > 0 && (
                            <div className="flex gap-1">
                              {linkedNote.tags.slice(0, 2).map((tag) => (
                                <Badge key={tag} variant="secondary" className="text-xs">
                                  {tag}
                                </Badge>
                              ))}
                              {linkedNote.tags.length > 2 && (
                                <Badge variant="secondary" className="text-xs">
                                  +{linkedNote.tags.length - 2}
                                </Badge>
                              )}
                            </div>
                          )}
                        </div>
                      </div>
                      <div className="flex items-center gap-2 ml-3">
                        {(linkedNote.hasReadAccess || linkedNote.isOwner) ? (
                          <Button
                            onClick={() => handleNavigateToLinkedNote(linkedNote.linkedNoteId)}
                            variant="outline"
                            size="sm"
                            className="border-white/20 text-white hover:bg-white/10"
                            title="Navigate to linked note"
                          >
                            <ExternalLink className="h-4 w-4" />
                          </Button>
                        ) : (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled
                            className="border-red-400/30 text-red-400 opacity-50"
                            title="No read access to this note"
                          >
                            <Ban className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </div>
                    {index < linkedNotes.length - 1 && (
                      <Separator className="bg-white/10 my-2" />
                    )}
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Linked Metadata Notes Section */}
      {linkedMetadataNotes.length > 0 && (
        <Card className="bg-white/10 backdrop-blur-sm border-white/20">
          <CardHeader>
            <CardTitle className="text-white flex items-center gap-2">
              <Database className="h-5 w-5 text-purple-400" />
              Linked Metadata Notes ({linkedMetadataNotes.length})
            </CardTitle>
            <CardDescription className="text-purple-200">
              Metadata notes that provide content substitution for this session note.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {loadingLinkedMetadata ? (
              <div className="flex items-center justify-center py-4">
                <Loader2 className="h-6 w-6 animate-spin text-purple-400" />
                <span className="ml-2 text-purple-200">Loading linked metadata notes...</span>
              </div>
            ) : (
              <div className="space-y-3">
                {linkedMetadataNotes.map((metadataNote, index) => {
                  const hasAccess = metadataAccessMap.get(metadataNote.id) || false;
                  
                  return (
                    <div key={metadataNote.id}>
                      <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <Database className="h-4 w-4 text-purple-400 flex-shrink-0" />
                            <span className="font-medium text-white truncate">
                              {metadataNote.name}
                            </span>
                            {!hasAccess && (
                              <Badge variant="outline" className="text-xs text-red-400 border-red-400/30">
                                No Access
                              </Badge>
                            )}
                          </div>
                          <div className="text-sm text-purple-200">
                            Metadata Note • {metadataNote.linkedSessionNotes.length} linked notes
                          </div>
                        </div>
                        <div className="flex items-center gap-2 ml-3">
                          {hasAccess ? (
                            <Button
                              onClick={() => handleNavigateToMetadataNote(metadataNote)}
                              variant="outline"
                              size="sm"
                              className="border-white/20 text-white hover:bg-white/10"
                              title="View metadata note details"
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                          ) : (
                            <Button
                              variant="outline"
                              size="sm"
                              disabled
                              className="border-red-400/30 text-red-400 opacity-50"
                              title="No access to this metadata note"
                            >
                              <Ban className="h-4 w-4" />
                            </Button>
                          )}
                          {hasWriteAccess && (
                            <AlertDialog>
                              <AlertDialogTrigger asChild>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  disabled={unlinkingMetadataId === metadataNote.id}
                                  className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
                                  title="Unlink metadata note"
                                >
                                  {unlinkingMetadataId === metadataNote.id ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                  ) : (
                                    <Unlink className="h-4 w-4" />
                                  )}
                                </Button>
                              </AlertDialogTrigger>
                              <AlertDialogContent className="bg-gray-900 border-orange-400/30 text-white">
                                <AlertDialogHeader>
                                  <AlertDialogTitle className="flex items-center gap-2">
                                    <Unlink className="h-5 w-5 text-orange-400" />
                                    Unlink Metadata Note
                                  </AlertDialogTitle>
                                  <AlertDialogDescription className="text-purple-200">
                                    Are you sure you want to unlink "{metadataNote.name}" from this session note? 
                                    This will remove the content substitution provided by this metadata note.
                                  </AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter>
                                  <AlertDialogCancel className="bg-white/10 border-white/20 text-white hover:bg-white/20">
                                    Cancel
                                  </AlertDialogCancel>
                                  <AlertDialogAction
                                    onClick={() => handleUnlinkMetadata(metadataNote)}
                                    className="bg-orange-600 hover:bg-orange-700 text-white border-0"
                                  >
                                    Unlink Metadata Note
                                  </AlertDialogAction>
                                </AlertDialogFooter>
                              </AlertDialogContent>
                            </AlertDialog>
                          )}
                        </div>
                      </div>
                      {index < linkedMetadataNotes.length - 1 && (
                        <Separator className="bg-white/10 my-2" />
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Access Management */}
      {isNoteOwner && (
        <Card className="bg-white/10 backdrop-blur-sm border-white/20">
          <CardHeader>
            <CardTitle className="text-white flex items-center gap-2">
              <Users className="h-5 w-5 text-amber-400" />
              Access Management
            </CardTitle>
            <CardDescription className="text-purple-200">
              Manage who can view and edit this note.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              onClick={() => setIsAccessDialogOpen(true)}
              variant="outline"
              className="border-white/20 text-white hover:bg-white/10"
            >
              <Users className="h-4 w-4 mr-2" />
              Manage Access
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Dialogs */}
      <EditNoteDialog
        note={note}
        open={isEditDialogOpen}
        onOpenChange={setIsEditDialogOpen}
        onNoteUpdated={onNoteUpdated}
      />

      <ManageNoteAccessDialog
        note={note}
        campaignParticipants={campaignParticipants}
        open={isAccessDialogOpen}
        onOpenChange={setIsAccessDialogOpen}
      />

      <NoteLinkingDialog
        note={note}
        campaignId={campaignId}
        open={isLinkingDialogOpen}
        onOpenChange={setIsLinkingDialogOpen}
        onNavigateToNote={onNavigateToNote}
        onLinksUpdated={handleLinksUpdated}
      />
    </div>
  );
}
