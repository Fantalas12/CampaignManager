import React, { useState, useEffect } from 'react';
import { useGetReceivedInvitations, useGetSentInvitations, useAcceptInvitation, useRejectInvitation, useGetUserProfile } from '../hooks/useQueries';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Mail, Check, X, Crown, Gamepad2, Clock, CheckCircle, XCircle, Loader2 } from 'lucide-react';
import { Invitation, InvitationStatus, Role } from '../backend';

interface InvitationsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

interface InvitationWithProfile extends Invitation {
  senderName?: string;
  recipientName?: string;
}

export default function InvitationsDialog({ open, onOpenChange }: InvitationsDialogProps) {
  const { data: receivedInvitations = [], refetch: refetchReceived } = useGetReceivedInvitations();
  const { data: sentInvitations = [], refetch: refetchSent } = useGetSentInvitations();
  const acceptInvitation = useAcceptInvitation();
  const rejectInvitation = useRejectInvitation();
  const getUserProfile = useGetUserProfile();

  const [receivedWithProfiles, setReceivedWithProfiles] = useState<InvitationWithProfile[]>([]);
  const [sentWithProfiles, setSentWithProfiles] = useState<InvitationWithProfile[]>([]);
  const [loadingProfiles, setLoadingProfiles] = useState(false);

  useEffect(() => {
    if (open) {
      loadInvitationsWithProfiles();
    }
  }, [open, receivedInvitations, sentInvitations]);

  const loadInvitationsWithProfiles = async () => {
    setLoadingProfiles(true);
    try {
      // Load received invitations with sender profiles
      const receivedWithNames = await Promise.all(
        receivedInvitations.map(async (invitation) => {
          try {
            const profile = await getUserProfile.mutateAsync(invitation.sender);
            return {
              ...invitation,
              senderName: profile?.name,
            };
          } catch {
            return invitation;
          }
        })
      );
      setReceivedWithProfiles(receivedWithNames);

      // Load sent invitations with recipient profiles
      const sentWithNames = await Promise.all(
        sentInvitations.map(async (invitation) => {
          try {
            const profile = await getUserProfile.mutateAsync(invitation.recipient);
            return {
              ...invitation,
              recipientName: profile?.name,
            };
          } catch {
            return invitation;
          }
        })
      );
      setSentWithProfiles(sentWithNames);
    } catch (error) {
      console.error('Failed to load invitation profiles:', error);
    } finally {
      setLoadingProfiles(false);
    }
  };

  const handleAccept = async (id: string) => {
    try {
      await acceptInvitation.mutateAsync(id);
      refetchReceived();
    } catch (error) {
      console.error('Failed to accept invitation:', error);
    }
  };

  const handleReject = async (id: string) => {
    try {
      await rejectInvitation.mutateAsync(id);
      refetchReceived();
    } catch (error) {
      console.error('Failed to reject invitation:', error);
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

  const getRoleText = (role: Role) => {
    switch (role) {
      case Role.gm:
        return 'GM';
      case Role.player:
        return 'Player';
      case Role.both:
        return 'GM & Player';
    }
  };

  const getStatusIcon = (status: InvitationStatus) => {
    switch (status) {
      case InvitationStatus.pending:
        return <Clock className="h-4 w-4 text-yellow-400" />;
      case InvitationStatus.accepted:
        return <CheckCircle className="h-4 w-4 text-green-400" />;
      case InvitationStatus.rejected:
        return <XCircle className="h-4 w-4 text-red-400" />;
    }
  };

  const getStatusText = (status: InvitationStatus) => {
    switch (status) {
      case InvitationStatus.pending:
        return 'Pending';
      case InvitationStatus.accepted:
        return 'Accepted';
      case InvitationStatus.rejected:
        return 'Rejected';
    }
  };

  const formatDate = (timestamp: bigint) => {
    return new Date(Number(timestamp / BigInt(1000000))).toLocaleDateString();
  };

  const pendingReceived = receivedWithProfiles.filter(inv => inv.status === InvitationStatus.pending);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-4xl max-h-[80vh]">
        <DialogHeader className="pb-4">
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Mail className="h-5 w-5 text-blue-400" />
            Invitations
            {pendingReceived.length > 0 && (
              <Badge variant="destructive" className="ml-2">
                {pendingReceived.length}
              </Badge>
            )}
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Manage your campaign invitations
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="received" className="w-full">
          <TabsList className="grid w-full grid-cols-2 bg-white/10">
            <TabsTrigger value="received" className="data-[state=active]:bg-white/20">
              Received ({receivedWithProfiles.length})
            </TabsTrigger>
            <TabsTrigger value="sent" className="data-[state=active]:bg-white/20">
              Sent ({sentWithProfiles.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="received" className="space-y-4 mt-4">
            <ScrollArea className="h-[500px] pr-4">
              {loadingProfiles ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                  <span className="ml-2 text-purple-200">Loading invitations...</span>
                </div>
              ) : receivedWithProfiles.length === 0 ? (
                <div className="text-center py-12">
                  <Mail className="h-16 w-16 text-purple-400 mx-auto mb-4" />
                  <p className="text-purple-200 text-lg mb-2">No invitations received</p>
                  <p className="text-purple-300 text-sm">You'll see campaign invitations here when you receive them</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {receivedWithProfiles.map((invitation) => (
                    <div
                      key={invitation.id}
                      className="p-4 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center space-x-2 mb-3">
                            {getRoleIcon(invitation.role)}
                            <h4 className="font-medium text-white">
                              Campaign Invitation
                            </h4>
                            <Badge variant="outline" className="text-xs">
                              {getRoleText(invitation.role)}
                            </Badge>
                          </div>
                          <div className="space-y-1 text-sm">
                            <p className="text-purple-200">
                              <span className="font-medium">From:</span> {invitation.senderName || 'Unknown User'}
                            </p>
                            <p className="text-purple-200">
                              <span className="font-medium">Campaign:</span> {invitation.campaignId}
                            </p>
                            <p className="text-purple-300 text-xs">
                              Received: {formatDate(invitation.timestamp)}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2 flex-shrink-0 ml-4">
                          {invitation.status === InvitationStatus.pending ? (
                            <>
                              <Button
                                onClick={() => handleAccept(invitation.id)}
                                size="sm"
                                className="bg-green-600 hover:bg-green-700 text-white"
                                disabled={acceptInvitation.isPending}
                              >
                                {acceptInvitation.isPending ? (
                                  <Loader2 className="h-4 w-4 animate-spin" />
                                ) : (
                                  <Check className="h-4 w-4 mr-1" />
                                )}
                                Accept
                              </Button>
                              <Button
                                onClick={() => handleReject(invitation.id)}
                                size="sm"
                                variant="outline"
                                className="border-red-400 text-red-400 hover:bg-red-500/10"
                                disabled={rejectInvitation.isPending}
                              >
                                {rejectInvitation.isPending ? (
                                  <Loader2 className="h-4 w-4 animate-spin" />
                                ) : (
                                  <X className="h-4 w-4 mr-1" />
                                )}
                                Reject
                              </Button>
                            </>
                          ) : (
                            <div className="flex items-center space-x-2">
                              {getStatusIcon(invitation.status)}
                              <span className="text-sm font-medium">{getStatusText(invitation.status)}</span>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </ScrollArea>
          </TabsContent>

          <TabsContent value="sent" className="space-y-4 mt-4">
            <ScrollArea className="h-[500px] pr-4">
              {loadingProfiles ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                  <span className="ml-2 text-purple-200">Loading invitations...</span>
                </div>
              ) : sentWithProfiles.length === 0 ? (
                <div className="text-center py-12">
                  <Mail className="h-16 w-16 text-purple-400 mx-auto mb-4" />
                  <p className="text-purple-200 text-lg mb-2">No invitations sent</p>
                  <p className="text-purple-300 text-sm">Invitations you send will appear here</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {sentWithProfiles.map((invitation) => (
                    <div
                      key={invitation.id}
                      className="p-4 bg-white/5 rounded-lg border border-white/10"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center space-x-2 mb-3">
                            {getRoleIcon(invitation.role)}
                            <h4 className="font-medium text-white">
                              Campaign Invitation
                            </h4>
                            <Badge variant="outline" className="text-xs">
                              {getRoleText(invitation.role)}
                            </Badge>
                          </div>
                          <div className="space-y-1 text-sm">
                            <p className="text-purple-200">
                              <span className="font-medium">To:</span> {invitation.recipientName || 'Unknown User'}
                            </p>
                            <p className="text-purple-200">
                              <span className="font-medium">Campaign:</span> {invitation.campaignId}
                            </p>
                            <p className="text-purple-300 text-xs">
                              Sent: {formatDate(invitation.timestamp)}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2 flex-shrink-0 ml-4">
                          {getStatusIcon(invitation.status)}
                          <span className="text-sm font-medium">{getStatusText(invitation.status)}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </ScrollArea>
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}
