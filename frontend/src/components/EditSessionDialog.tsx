import React, { useState, useEffect } from 'react';
import { Session } from '../backend';
import { useUpdateSession } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Edit, Calendar, Sparkles, Crown } from 'lucide-react';

interface EditSessionDialogProps {
  session: Session;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSessionUpdated?: () => void;
}

export default function EditSessionDialog({ session, open, onOpenChange, onSessionUpdated }: EditSessionDialogProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [date, setDate] = useState('');
  const updateSession = useUpdateSession();

  useEffect(() => {
    if (open) {
      setName(session.name);
      setDescription(session.description);
      setDate(session.date);
    }
  }, [open, session]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (name.trim() && date.trim()) {
      updateSession.mutate(
        { 
          id: session.id, 
          name: name.trim(), 
          description: description.trim(), 
          date: date.trim() 
        },
        {
          onSuccess: () => {
            onOpenChange(false);
            onSessionUpdated?.();
          },
        }
      );
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setName(session.name);
      setDescription(session.description);
      setDate(session.date);
    }
    onOpenChange(newOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Edit className="h-5 w-5 text-blue-400" />
            Edit Session
            <Sparkles className="h-4 w-4 text-purple-400" />
          </DialogTitle>
          <DialogDescription className="text-purple-200 flex items-start gap-2">
            <div className="flex-1">
              Update your session details. Make changes to keep your adventure records accurate.
            </div>
            <div className="flex items-center gap-1 text-xs text-amber-300 bg-amber-500/10 px-2 py-1 rounded border border-amber-400/20 flex-shrink-0">
              <Crown className="h-3 w-3" />
              <span>Owner Only</span>
            </div>
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="edit-session-name" className="text-white">Session Name</Label>
            <Input
              id="edit-session-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter session name"
              required
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 h-12"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-session-description" className="text-white">Description</Label>
            <Textarea
              id="edit-session-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe what happens in this session, key events, or notes..."
              rows={4}
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 resize-none"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-session-date" className="text-white flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Real-World Date
            </Label>
            <Input
              id="edit-session-date"
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              required
              className="bg-white/10 border-white/20 text-white h-12"
            />
            <p className="text-xs text-purple-300">
              When did this session take place in real life?
            </p>
          </div>
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
              disabled={!name.trim() || !date.trim() || updateSession.isPending}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              {updateSession.isPending ? 'Updating...' : 'Update Session'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
