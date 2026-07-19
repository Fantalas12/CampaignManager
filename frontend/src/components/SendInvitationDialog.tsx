import React, { useState } from 'react';
import { Campaign, Role } from '../backend';
import { useSendInvitation } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Mail, Crown, Gamepad2, AlertCircle, Loader2, User } from 'lucide-react';

interface SendInvitationDialogProps {
  campaign: Campaign;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onInviteSent?: () => void;
}

export default function SendInvitationDialog({ campaign, open, onOpenChange, onInviteSent }: SendInvitationDialogProps) {
  const [recipientUsername, setRecipientUsername] = useState('');
  const [selectedRole, setSelectedRole] = useState<Role>(Role.player);
  const [error, setError] = useState('');
  const sendInvitation = useSendInvitation();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!recipientUsername.trim()) {
      setError('Please enter a username');
      return;
    }

    try {
      const invitationId = `invitation_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
      
      await sendInvitation.mutateAsync({
        id: invitationId,
        campaignId: campaign.id,
        recipientUsername: recipientUsername.trim(),
        role: selectedRole,
      });

      setRecipientUsername('');
      setSelectedRole(Role.player);
      onOpenChange(false);
      onInviteSent?.();
    } catch (err: any) {
      if (err.message.includes('Recipient not found')) {
        setError('Username not found. Please check the username and try again.');
      } else {
        setError(err.message || 'Failed to send invitation');
      }
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      setRecipientUsername('');
      setSelectedRole(Role.player);
      setError('');
    }
    onOpenChange(newOpen);
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

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Mail className="h-5 w-5 text-blue-400" />
            Send Invitation
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Invite a user to join "{campaign.name}"
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="recipient" className="text-white flex items-center gap-2">
              <User className="h-4 w-4" />
              Username
            </Label>
            <Input
              id="recipient"
              type="text"
              value={recipientUsername}
              onChange={(e) => setRecipientUsername(e.target.value)}
              placeholder="Enter username (e.g., john_doe)"
              required
              className="bg-white/10 border-white/20 text-white placeholder:text-purple-300"
            />
            <p className="text-xs text-purple-300">
              Enter the username of the person you want to invite
            </p>
          </div>

          <div className="space-y-2">
            <Label className="text-white">Role Assignment</Label>
            <Select value={selectedRole} onValueChange={(value) => setSelectedRole(value as Role)}>
              <SelectTrigger className="bg-white/10 border-white/20 text-white">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-gray-900 border-white/20">
                <SelectItem value={Role.player} className="text-white hover:bg-white/10">
                  <div className="flex items-center space-x-2">
                    <Gamepad2 className="h-4 w-4 text-blue-400" />
                    <span>Player</span>
                  </div>
                </SelectItem>
                <SelectItem value={Role.gm} className="text-white hover:bg-white/10">
                  <div className="flex items-center space-x-2">
                    <Crown className="h-4 w-4 text-amber-400" />
                    <span>Game Master</span>
                  </div>
                </SelectItem>
                <SelectItem value={Role.both} className="text-white hover:bg-white/10">
                  <div className="flex items-center space-x-2">
                    <Crown className="h-3 w-3 text-amber-400" />
                    <Gamepad2 className="h-3 w-3 text-blue-400" />
                    <span>GM & Player</span>
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
            <p className="text-xs text-purple-300">
              Choose the role this user will have in your campaign
            </p>
          </div>

          {error && (
            <Alert className="bg-red-500/10 border-red-400/20">
              <AlertCircle className="h-4 w-4 text-red-400" />
              <AlertDescription className="text-red-300">
                {error}
              </AlertDescription>
            </Alert>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              className="border-white/20 text-white hover:bg-white/10"
              disabled={sendInvitation.isPending}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={!recipientUsername.trim() || sendInvitation.isPending}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              {sendInvitation.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Sending...
                </>
              ) : (
                <>
                  <Mail className="h-4 w-4 mr-2" />
                  Send Invitation
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
