import React, { useState } from 'react';
import { Campaign } from '../backend';
import { useCreateSession } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Clock, Calendar, Sparkles, Crown } from 'lucide-react';

interface CreateSessionDialogProps {
  campaign: Campaign;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSessionCreated?: () => void;
}

export default function CreateSessionDialog({ campaign, open, onOpenChange, onSessionCreated }: CreateSessionDialogProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [date, setDate] = useState('');
  const createSession = useCreateSession();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (name.trim() && date.trim()) {
      const id = `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
      createSession.mutate(
        { 
          id, 
          campaignId: campaign.id, 
          name: name.trim(), 
          description: description.trim(), 
          date: date.trim() 
        },
        {
          onSuccess: () => {
            setName('');
            setDescription('');
            setDate('');
            onOpenChange(false);
            onSessionCreated?.();
          },
        }
      );
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setName('');
      setDescription('');
      setDate('');
    }
    onOpenChange(newOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Clock className="h-5 w-5 text-blue-400" />
            Create New Session
            <Sparkles className="h-4 w-4 text-purple-400" />
          </DialogTitle>
          <DialogDescription className="text-purple-200 flex items-start gap-2">
            <div className="flex-1">
              Create a new session for "{campaign.name}". Plan your next adventure!
            </div>
            <div className="flex items-center gap-1 text-xs text-amber-300 bg-amber-500/10 px-2 py-1 rounded border border-amber-400/20 flex-shrink-0">
              <Crown className="h-3 w-3" />
              <span>GM Only</span>
            </div>
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="session-name" className="text-white">Session Name</Label>
            <Input
              id="session-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter session name (e.g., 'The Dragon's Lair')"
              required
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 h-12"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="session-description" className="text-white">Description</Label>
            <Textarea
              id="session-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe what happens in this session, key events, or notes..."
              rows={4}
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300 resize-none"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="session-date" className="text-white flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Real-World Date
            </Label>
            <Input
              id="session-date"
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
              disabled={!name.trim() || !date.trim() || createSession.isPending}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              {createSession.isPending ? 'Creating...' : 'Create Session'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
