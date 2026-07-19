import React, { useState, useEffect } from 'react';
import { Note, LinkedNoteDetails } from '../backend';
import { useLinkNotes, useGetLinkedNotes, useRemoveNoteLink } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Link, Unlink, Plus, FileText, Calendar, Loader2, AlertTriangle, ExternalLink, Info, Ban } from 'lucide-react';
import { toast } from 'sonner';

interface NoteLinkingDialogProps {
  note: Note;
  campaignId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onNavigateToNote?: (noteId: string) => void;
  onLinksUpdated?: () => void;
}

export default function NoteLinkingDialog({ note, campaignId, open, onOpenChange, onNavigateToNote, onLinksUpdated }: NoteLinkingDialogProps) {
  const [targetNoteName, setTargetNoteName] = useState('');
  const [linkedNotes, setLinkedNotes] = useState<LinkedNoteDetails[]>([]);
  const [loadingLinks, setLoadingLinks] = useState(false);
  const [removingLinkId, setRemovingLinkId] = useState<string | null>(null);
  const [isRemoveDialogOpen, setIsRemoveDialogOpen] = useState(false);
  const [noteToRemove, setNoteToRemove] = useState<LinkedNoteDetails | null>(null);

  const linkNotes = useLinkNotes();
  const getLinkedNotes = useGetLinkedNotes();
  const removeNoteLink = useRemoveNoteLink();

  useEffect(() => {
    if (open) {
      loadLinkedNotes();
    }
  }, [open, note.id]);

  const loadLinkedNotes = async () => {
    setLoadingLinks(true);
    try {
      const links = await getLinkedNotes.mutateAsync(note.id);
      setLinkedNotes(links);
    } catch (error) {
      console.error('Failed to load linked notes:', error);
      setLinkedNotes([]);
    } finally {
      setLoadingLinks(false);
    }
  };

  const handleLinkNote = async () => {
    if (!targetNoteName.trim()) return;

    // Check if user is trying to link note to itself
    if (targetNoteName.trim() === note.name) {
      toast.error('Cannot link a note to itself. Please choose a different note name.');
      return;
    }

    // Check if link already exists with this note name
    const existingLink = linkedNotes.find(
      link => link.noteName.toLowerCase() === targetNoteName.trim().toLowerCase()
    );
    if (existingLink) {
      toast.error('A link already exists between these notes. Please choose a different note name.');
      return;
    }

    try {
      await linkNotes.mutateAsync({
        noteId: note.id,
        targetNoteName: targetNoteName.trim(),
      });
      setTargetNoteName('');
      await loadLinkedNotes(); // Refresh the linked notes list
      onLinksUpdated?.(); // Notify parent component
    } catch (error) {
      // Error is already handled by the mutation's onError
    }
  };

  const handleRemoveLink = (linkedNote: LinkedNoteDetails) => {
    setNoteToRemove(linkedNote);
    setIsRemoveDialogOpen(true);
  };

  const confirmRemoveLink = async () => {
    if (!noteToRemove) return;

    setRemovingLinkId(noteToRemove.linkedNoteId);
    try {
      await removeNoteLink.mutateAsync({
        noteId: note.id,
        linkedNoteId: noteToRemove.linkedNoteId,
      });
      await loadLinkedNotes(); // Refresh the linked notes list
      onLinksUpdated?.(); // Notify parent component
    } catch (error) {
      // Error is already handled by the mutation's onError
    } finally {
      setRemovingLinkId(null);
      setIsRemoveDialogOpen(false);
      setNoteToRemove(null);
    }
  };

  const handleNavigateToLinkedNote = (linkedNoteId: string) => {
    onNavigateToNote?.(linkedNoteId);
    onOpenChange(false);
  };

  // Check if the target note name is the same as current note
  const isSelfLinking = targetNoteName.trim() === note.name;

  // Check if a link already exists with this note name
  const isDuplicateLink = linkedNotes.some(
    link => link.noteName.toLowerCase() === targetNoteName.trim().toLowerCase()
  );

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="bg-gray-900 border-white/20 text-white max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-xl">
              <Link className="h-6 w-6 text-blue-400" />
              Manage Session Note Links
            </DialogTitle>
            <DialogDescription className="text-purple-200">
              Manage links between this session note and other session notes within the same campaign.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-6">
            {/* Current Note Info */}
            <Card className="bg-white/5 border-white/10">
              <CardHeader className="pb-3">
                <CardTitle className="text-lg text-white flex items-center gap-2">
                  <FileText className="h-5 w-5 text-green-400" />
                  Current Note
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  <div className="font-medium text-white">{note.name}</div>
                  {note.inGameDateTime && (
                    <div className="flex items-center gap-1 text-sm text-purple-200">
                      <Calendar className="h-4 w-4" />
                      {note.inGameDateTime}
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>

            {/* Session Note Links Section */}
            <div className="space-y-4">
              {/* Add New Session Note Link */}
              <Card className="bg-white/5 border-white/10">
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg text-white flex items-center gap-2">
                    <Plus className="h-5 w-5 text-blue-400" />
                    Link Session Note
                  </CardTitle>
                  <CardDescription className="text-purple-200">
                    Enter the exact name of another session note in this campaign to create a link.
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <Alert className="bg-blue-500/10 border-blue-400/20">
                    <Info className="h-4 w-4 text-blue-400" />
                    <AlertDescription className="text-blue-200 text-sm">
                      <strong>Note linking uses exact note names.</strong> Make sure to type the target note's name exactly as it appears, including capitalization and spacing.
                    </AlertDescription>
                  </Alert>

                  {/* Self-linking prevention warning */}
                  {isSelfLinking && (
                    <Alert className="bg-red-500/10 border-red-400/20">
                      <Ban className="h-4 w-4 text-red-400" />
                      <AlertDescription className="text-red-200 text-sm">
                        <strong>Cannot link to itself:</strong> You cannot link a note to itself. Please enter the name of a different note.
                      </AlertDescription>
                    </Alert>
                  )}

                  {/* Duplicate link prevention warning */}
                  {!isSelfLinking && isDuplicateLink && (
                    <Alert className="bg-red-500/10 border-red-400/20">
                      <Ban className="h-4 w-4 text-red-400" />
                      <AlertDescription className="text-red-200 text-sm">
                        <strong>Link already exists:</strong> A link already exists between these notes. Please enter the name of a different note.
                      </AlertDescription>
                    </Alert>
                  )}

                  <div className="space-y-2">
                    <Label htmlFor="targetNoteName" className="text-white">
                      Target Session Note Name
                    </Label>
                    <Input
                      id="targetNoteName"
                      value={targetNoteName}
                      onChange={(e) => setTargetNoteName(e.target.value)}
                      placeholder="Enter the exact session note name..."
                      className={`bg-white/10 border-white/20 text-white placeholder:text-purple-300 ${
                        isSelfLinking || isDuplicateLink ? 'border-red-400/50 bg-red-500/5' : ''
                      }`}
                      disabled={linkNotes.isPending}
                    />
                  </div>
                  <Button
                    onClick={handleLinkNote}
                    disabled={!targetNoteName.trim() || linkNotes.isPending || isSelfLinking || isDuplicateLink}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white disabled:opacity-50"
                  >
                    {linkNotes.isPending ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Linking...
                      </>
                    ) : isSelfLinking ? (
                      <>
                        <Ban className="h-4 w-4 mr-2" />
                        Cannot Link to Self
                      </>
                    ) : isDuplicateLink ? (
                      <>
                        <Ban className="h-4 w-4 mr-2" />
                        Link Already Exists
                      </>
                    ) : (
                      <>
                        <Link className="h-4 w-4 mr-2" />
                        Create Link
                      </>
                    )}
                  </Button>
                </CardContent>
              </Card>

              {/* Existing Session Note Links */}
              <Card className="bg-white/5 border-white/10">
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg text-white flex items-center gap-2">
                    <Link className="h-5 w-5 text-green-400" />
                    Linked Session Notes ({linkedNotes.length})
                  </CardTitle>
                  <CardDescription className="text-purple-200">
                    Session notes that are currently linked to this note.
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {loadingLinks ? (
                    <div className="flex items-center justify-center py-8">
                      <Loader2 className="h-6 w-6 animate-spin text-purple-400" />
                      <span className="ml-2 text-purple-200">Loading linked notes...</span>
                    </div>
                  ) : linkedNotes.length === 0 ? (
                    <div className="text-center py-8">
                      <Link className="h-12 w-12 text-purple-400 mx-auto mb-3" />
                      <h4 className="text-lg font-medium text-white mb-2">No Linked Session Notes</h4>
                      <p className="text-purple-200 text-sm">
                        This note isn't linked to any other session notes yet.
                      </p>
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
                                  <ExternalLink className="h-4 w-4" />
                                </Button>
                              )}
                              <Button
                                onClick={() => handleRemoveLink(linkedNote)}
                                variant="outline"
                                size="sm"
                                disabled={removingLinkId === linkedNote.linkedNoteId}
                                className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
                                title="Remove link"
                              >
                                {removingLinkId === linkedNote.linkedNoteId ? (
                                  <Loader2 className="h-4 w-4 animate-spin" />
                                ) : (
                                  <Unlink className="h-4 w-4" />
                                )}
                              </Button>
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
            </div>
          </div>

          <DialogFooter>
            <Button
              onClick={() => onOpenChange(false)}
              variant="outline"
              className="border-white/20 text-white hover:bg-white/10"
            >
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Remove Session Note Link Confirmation Dialog */}
      <AlertDialog open={isRemoveDialogOpen} onOpenChange={setIsRemoveDialogOpen}>
        <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white max-w-md">
          <AlertDialogHeader className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="bg-red-500/20 p-3 rounded-full">
                <AlertTriangle className="h-8 w-8 text-red-400" />
              </div>
            </div>
            <AlertDialogTitle className="text-center text-xl font-bold text-white">
              Remove Session Note Link
            </AlertDialogTitle>
            <AlertDialogDescription className="text-center text-purple-200 space-y-2">
              <p className="font-medium">
                Are you sure you want to remove the link to <span className="text-white font-semibold">"{noteToRemove?.noteName}"</span>?
              </p>
              <p className="text-sm text-red-300">
                This will remove the bidirectional link between both notes. This action cannot be undone.
              </p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <AlertDialogCancel 
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
              disabled={removingLinkId !== null}
            >
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmRemoveLink}
              disabled={removingLinkId !== null}
              className="bg-red-600 hover:bg-red-700 text-white border-0 order-1 sm:order-2"
            >
              {removingLinkId !== null ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Removing...
                </>
              ) : (
                <>
                  <Unlink className="h-4 w-4 mr-2" />
                  Remove Link
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
