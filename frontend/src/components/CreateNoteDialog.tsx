import React, { useState } from 'react';
import { Session } from '../backend';
import { useCreateNote } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { FileText, Sparkles, Crown, Gamepad2, Eye, Info, Link } from 'lucide-react';
import DateTimeInput from './DateTimeInput';
import TagInput from './TagInput';

interface CreateNoteDialogProps {
  session: Session;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onNoteCreated?: () => void;
}

export default function CreateNoteDialog({ session, open, onOpenChange, onNoteCreated }: CreateNoteDialogProps) {
  const [name, setName] = useState('');
  const [playerContent, setPlayerContent] = useState('');
  const [gmContent, setGmContent] = useState('');
  const [inGameDateTime, setInGameDateTime] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const createNote = useCreateNote();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (name.trim() && (playerContent.trim() || gmContent.trim())) {
      const id = `note_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
      createNote.mutate(
        { 
          id, 
          sessionId: session.id, 
          name: name.trim(), 
          playerContent: playerContent.trim(),
          gmContent: gmContent.trim(),
          inGameDateTime: inGameDateTime.trim() || undefined,
          tags
        },
        {
          onSuccess: () => {
            setName('');
            setPlayerContent('');
            setGmContent('');
            setInGameDateTime('');
            setTags([]);
            onOpenChange(false);
            onNoteCreated?.();
          },
        }
      );
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setName('');
      setPlayerContent('');
      setGmContent('');
      setInGameDateTime('');
      setTags([]);
    }
    onOpenChange(newOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <FileText className="h-5 w-5 text-green-400" />
            Create New Note
            <Sparkles className="h-4 w-4 text-purple-400" />
          </DialogTitle>
          <DialogDescription className="text-purple-200 flex items-start gap-2">
            <div className="flex-1">
              Create a new note for "{session.name}". You can provide different content for players and GMs to control what each group sees.
            </div>
            <div className="flex items-center gap-1 text-xs text-amber-300 bg-amber-500/10 px-2 py-1 rounded border border-amber-400/20 flex-shrink-0">
              <Crown className="h-3 w-3" />
              <span>Session Owner Only</span>
            </div>
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="note-name" className="text-white">Note Name</Label>
            <Input
              id="note-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter note name (e.g., 'Combat with Goblins', 'Important NPC Dialogue')"
              required
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 h-12"
            />
            <Alert className="bg-blue-500/10 border-blue-400/20">
              <Info className="h-4 w-4 text-blue-400" />
              <AlertDescription className="text-blue-200 text-sm">
                <strong>Note names must be unique within this campaign</strong> and are used for linking notes together. Choose a descriptive, unique name that you can easily reference when creating links to this note.
              </AlertDescription>
            </Alert>
          </div>

          <TagInput
            tags={tags}
            onTagsChange={setTags}
            label="Tags (Optional)"
            placeholder="Add a tag to categorize this note..."
          />

          <Separator className="bg-white/20" />

          <div className="grid md:grid-cols-2 gap-6">
            {/* Player Content */}
            <div className="space-y-2">
              <Label htmlFor="player-content" className="text-white flex items-center gap-2">
                <Gamepad2 className="h-4 w-4 text-blue-400" />
                Player Content
              </Label>
              <Textarea
                id="player-content"
                value={playerContent}
                onChange={(e) => setPlayerContent(e.target.value)}
                placeholder="Content that players will see. Include information that should be visible to all players..."
                rows={8}
                className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 resize-none"
              />
              <p className="text-xs text-blue-300 flex items-center gap-1">
                <Eye className="h-3 w-3" />
                Visible to users with Player Read permission
              </p>
            </div>

            {/* GM Content */}
            <div className="space-y-2">
              <Label htmlFor="gm-content" className="text-white flex items-center gap-2">
                <Crown className="h-4 w-4 text-amber-400" />
                GM Content
              </Label>
              <Textarea
                id="gm-content"
                value={gmContent}
                onChange={(e) => setGmContent(e.target.value)}
                placeholder="Content that only GMs will see. Include secret information, behind-the-scenes details, or GM-only notes..."
                rows={8}
                className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 resize-none"
              />
              <p className="text-xs text-amber-300 flex items-center gap-1">
                <Eye className="h-3 w-3" />
                Visible to users with GM Read permission
              </p>
            </div>
          </div>

          <div className="bg-blue-500/10 border border-blue-400/20 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <FileText className="h-4 w-4 text-blue-400 mt-0.5 flex-shrink-0" />
              <div className="text-sm text-blue-200">
                <p className="font-medium mb-1">Content Visibility</p>
                <p>You can fill in both fields or just one. Users will only see content based on their assigned permissions. Use the access management feature after creating the note to control who can see what.</p>
              </div>
            </div>
          </div>

          <div className="bg-green-500/10 border border-green-400/20 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <Link className="h-4 w-4 text-green-400 mt-0.5 flex-shrink-0" />
              <div className="text-sm text-green-200">
                <p className="font-medium mb-1">Note Linking</p>
                <p>After creating this note, you can link it to other notes in the same campaign by using the exact note name. Links work both ways - when you link to another note, both notes will reference each other.</p>
              </div>
            </div>
          </div>

          <DateTimeInput
            value={inGameDateTime}
            onChange={setInGameDateTime}
            placeholder="e.g., March 15, 1423 14:30 (optional)"
            label="In-Game Date & Time (Optional)"
          />

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              className="border-white/20 text-white hover:bg-white/10"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={!name.trim() || (!playerContent.trim() && !gmContent.trim()) || createNote.isPending}
              className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white border-0"
            >
              {createNote.isPending ? 'Creating...' : 'Create Note'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
