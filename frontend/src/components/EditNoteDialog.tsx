import React, { useState, useEffect } from 'react';
import { Note } from '../backend';
import { useUpdateNote, useGetMetadataNotes } from '../hooks/useQueries';
import { processContentSubstitution, hasPlaceholders } from '../lib/dynamicContentSubstitution';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Loader2, FileText, Gamepad2, Crown, Calendar, Info, Link, Eye, Edit3 } from 'lucide-react';
import { validateInGameDateTime } from '../lib/dateValidation';
import TagInput from './TagInput';

interface EditNoteDialogProps {
  note: Note;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onNoteUpdated?: () => void;
}

export default function EditNoteDialog({ note, open, onOpenChange, onNoteUpdated }: EditNoteDialogProps) {
  const [name, setName] = useState('');
  const [playerContent, setPlayerContent] = useState('');
  const [gmContent, setGmContent] = useState('');
  const [inGameDateTime, setInGameDateTime] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [dateError, setDateError] = useState('');
  
  const updateNote = useUpdateNote();
  const { data: metadataNotes = [] } = useGetMetadataNotes();

  // Initialize form with note data when dialog opens
  useEffect(() => {
    if (open && note) {
      setName(note.name);
      setPlayerContent(note.playerContent);
      setGmContent(note.gmContent);
      setInGameDateTime(note.inGameDateTime || '');
      setTags([...note.tags]);
      setDateError('');
    }
  }, [open, note]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!name.trim()) {
      return;
    }

    // Validate in-game date/time if provided
    if (inGameDateTime.trim()) {
      const validation = validateInGameDateTime(inGameDateTime.trim());
      if (!validation.isValid) {
        setDateError(validation.error || 'Invalid date format');
        return;
      }
    }

    setDateError('');

    updateNote.mutate({
      id: note.id,
      name: name.trim(),
      playerContent: playerContent.trim(),
      gmContent: gmContent.trim(),
      inGameDateTime: inGameDateTime.trim() || undefined,
      tags
    }, {
      onSuccess: () => {
        onOpenChange(false);
        onNoteUpdated?.();
        // Reset form
        setName('');
        setPlayerContent('');
        setGmContent('');
        setInGameDateTime('');
        setTags([]);
        setDateError('');
      }
    });
  };

  const handleCancel = () => {
    onOpenChange(false);
    // Reset form to original values
    setName(note.name);
    setPlayerContent(note.playerContent);
    setGmContent(note.gmContent);
    setInGameDateTime(note.inGameDateTime || '');
    setTags([...note.tags]);
    setDateError('');
  };

  // Process content for preview
  const processedPlayerContent = playerContent ? processContentSubstitution(playerContent, metadataNotes) : '';
  const processedGmContent = gmContent ? processContentSubstitution(gmContent, metadataNotes) : '';
  
  // Check for placeholders
  const playerHasPlaceholders = hasPlaceholders(playerContent);
  const gmHasPlaceholders = hasPlaceholders(gmContent);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <FileText className="h-6 w-6 text-green-400" />
            Edit Note
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Update the note content and settings. Both player and GM content fields support HTML tags and content substitution.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="name" className="text-white">Note Name *</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter note name..."
              className="bg-white/10 border-white/20 text-white placeholder:text-white/50"
              required
            />
            <Alert className="bg-blue-500/10 border-blue-400/20">
              <Info className="h-4 w-4 text-blue-400" />
              <AlertDescription className="text-blue-200 text-sm">
                <strong>Note names must be unique within this campaign</strong> and are used for linking notes together. Changing the name may affect existing links to this note.
              </AlertDescription>
            </Alert>
          </div>

          <TagInput
            tags={tags}
            onTagsChange={setTags}
            label="Tags"
            placeholder="Add a tag to categorize this note..."
          />

          <div className="space-y-2">
            <Label htmlFor="inGameDateTime" className="text-white flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              In-Game Date & Time (Optional)
            </Label>
            <Input
              id="inGameDateTime"
              value={inGameDateTime}
              onChange={(e) => {
                setInGameDateTime(e.target.value);
                if (dateError) setDateError('');
              }}
              placeholder="e.g., 15th day of Mirtul, 1372 DR, evening"
              className="bg-white/10 border-white/20 text-white placeholder:text-white/50"
            />
            {dateError && (
              <p className="text-red-400 text-sm">{dateError}</p>
            )}
            <p className="text-purple-300 text-xs">
              Enter any date/time format that makes sense for your campaign world.
            </p>
          </div>

          <div className="bg-green-500/10 border border-green-400/20 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <Link className="h-4 w-4 text-green-400 mt-0.5 flex-shrink-0" />
              <div className="text-sm text-green-200">
                <p className="font-medium mb-1">Note Linking & Content Substitution</p>
                <p>This note can be linked to other notes in the same campaign using its exact name "{name || 'current note name'}". 
                You can also use placeholders like <code className="bg-black/30 px-1 py-0.5 rounded">&lt;---field---&gt;</code> to 
                reference metadata note values that will be automatically substituted when displayed.</p>
              </div>
            </div>
          </div>

          <Separator className="bg-white/20" />

          {/* Content Editing with Preview */}
          <Tabs defaultValue="edit" className="w-full">
            <TabsList className="grid w-full grid-cols-2 bg-white/10">
              <TabsTrigger value="edit" className="flex items-center gap-2">
                <Edit3 className="h-4 w-4" />
                Edit Mode
              </TabsTrigger>
              <TabsTrigger value="preview" className="flex items-center gap-2">
                <Eye className="h-4 w-4" />
                Preview Mode
              </TabsTrigger>
            </TabsList>

            <TabsContent value="edit" className="space-y-6 mt-6">
              {/* Player Content - Edit Mode */}
              <div className="space-y-4">
                <div className="flex items-center gap-2 text-blue-300">
                  <Gamepad2 className="h-5 w-5" />
                  <h3 className="text-lg font-semibold">Player Content</h3>
                  {playerHasPlaceholders && (
                    <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                      Contains placeholders
                    </Badge>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="playerContent" className="text-white">
                    Content visible to players with Player Read permission
                  </Label>
                  <Textarea
                    id="playerContent"
                    value={playerContent}
                    onChange={(e) => setPlayerContent(e.target.value)}
                    placeholder="Enter content that players should see... Use <---field---> for content substitution."
                    className="bg-blue-500/10 border-blue-400/20 text-white placeholder:text-white/50 min-h-[120px] resize-y font-mono text-sm"
                  />
                </div>
              </div>

              <Separator className="bg-white/20" />

              {/* GM Content - Edit Mode */}
              <div className="space-y-4">
                <div className="flex items-center gap-2 text-amber-300">
                  <Crown className="h-5 w-5" />
                  <h3 className="text-lg font-semibold">GM Content</h3>
                  {gmHasPlaceholders && (
                    <Badge variant="outline" className="text-xs text-amber-300 border-amber-400/30">
                      Contains placeholders
                    </Badge>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="gmContent" className="text-white">
                    Content visible to users with GM Read permission
                  </Label>
                  <Textarea
                    id="gmContent"
                    value={gmContent}
                    onChange={(e) => setGmContent(e.target.value)}
                    placeholder="Enter GM-only content, secret information, behind-the-scenes notes... Use <---field---> for content substitution."
                    className="bg-amber-500/10 border-amber-400/20 text-white placeholder:text-white/50 min-h-[120px] resize-y font-mono text-sm"
                  />
                </div>
              </div>
            </TabsContent>

            <TabsContent value="preview" className="space-y-6 mt-6">
              {/* Player Content - Preview Mode */}
              {playerContent && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2 text-blue-300">
                    <Gamepad2 className="h-5 w-5" />
                    <h3 className="text-lg font-semibold">Player Content Preview</h3>
                    {playerHasPlaceholders && (
                      <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                        Processed
                      </Badge>
                    )}
                  </div>
                  <div className="bg-blue-500/10 border border-blue-400/20 rounded-lg p-4">
                    <div 
                      className="text-white whitespace-pre-wrap"
                      dangerouslySetInnerHTML={{ __html: processedPlayerContent }}
                    />
                  </div>
                </div>
              )}

              {playerContent && gmContent && <Separator className="bg-white/20" />}

              {/* GM Content - Preview Mode */}
              {gmContent && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2 text-amber-300">
                    <Crown className="h-5 w-5" />
                    <h3 className="text-lg font-semibold">GM Content Preview</h3>
                    {gmHasPlaceholders && (
                      <Badge variant="outline" className="text-xs text-amber-300 border-amber-400/30">
                        Processed
                      </Badge>
                    )}
                  </div>
                  <div className="bg-amber-500/10 border border-amber-400/20 rounded-lg p-4">
                    <div 
                      className="text-white whitespace-pre-wrap"
                      dangerouslySetInnerHTML={{ __html: processedGmContent }}
                    />
                  </div>
                </div>
              )}

              {!playerContent && !gmContent && (
                <div className="text-center py-8 text-purple-300">
                  <FileText className="h-12 w-12 mx-auto mb-2 opacity-50" />
                  <p>No content to preview. Add content in Edit Mode to see the preview here.</p>
                </div>
              )}
            </TabsContent>
          </Tabs>

          <DialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={updateNote.isPending}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={updateNote.isPending || !name.trim()}
              className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white border-0 order-1 sm:order-2"
            >
              {updateNote.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Updating Note...
                </>
              ) : (
                <>
                  <FileText className="h-4 w-4 mr-2" />
                  Update Note
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
